using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IDatabaseService
    {
        Task<(bool Ok, string Mensaje)> VerificarConexionAsync(DatabaseConfig config, int timeoutSeg = 5, CancellationToken cancellationToken = default);

        // Métodos optimizados usando callback para streaming simulado
        Task ConsultarTransaccionesFrontAsync(DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada);
        Task ConsultarTransaccionesBackAsync(DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada);

        // Métodos de conteo
        Task<int> ContarTransaccionesFrontAsync(DatabaseConfig config, int anio);
        Task<int> ContarTransaccionesBackAsync(DatabaseConfig config, int anio);
    }

    public class DatabaseService : IDatabaseService
    {
        public async Task<(bool Ok, string Mensaje)> VerificarConexionAsync(
            DatabaseConfig config,
            int timeoutSeg = 3,
            CancellationToken cancellationToken = default)
        {
            if (config == null || !config.EstaCompleto())
                return (false, "Configuración de base de datos incompleta");

            var csb = new SqlConnectionStringBuilder(config.GetConnectionString())
            {
                ConnectTimeout = timeoutSeg,
                Encrypt = false,
                TrustServerCertificate = true
            };

            try
            {
                using (var connection = new SqlConnection(csb.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        var result = await command.ExecuteScalarAsync(cancellationToken);
                        return (Convert.ToInt32(result) == 1, "Conexión exitosa");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return (false, "Tiempo de espera agotado");
            }
            catch (SqlException ex)
            {
                return (false, $"Error SQL ({ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task ConsultarTransaccionesFrontAsync(
            DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Front incompleta");

            // Procesar mes por mes para controlar memoria
            for (int mes = 1; mes <= 12; mes++)
            {
                await ConsultarTransaccionesFrontPorMesAsync(config, anio, mes, onTransaccionProcesada);

                // Pausa pequeña entre meses para no saturar la BD
                await Task.Delay(50);

                // Forzar liberación de memoria cada mes
                GC.Collect();
            }
        }

        public async Task ConsultarTransaccionesBackAsync(
            DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Back incompleta");

            // Procesar mes por mes para controlar memoria
            for (int mes = 1; mes <= 12; mes++)
            {
                await ConsultarTransaccionesBackPorMesAsync(config, anio, mes, onTransaccionProcesada);

                await Task.Delay(50);
                GC.Collect();
            }
        }

        private async Task ConsultarTransaccionesFrontPorMesAsync(
            DatabaseConfig config, int anio, int mes, Func<TransaccionData, Task> onTransaccionProcesada)
        {
            using (var connection = new SqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();

                const string query = @"
                    SELECT Modulo, Mensaje_numero_cd, ParametrosAdicionales_Xml, Fechahoragrabacion 
                    FROM Ho_facturaelectronica_transaccion 
                    WHERE Operacion = 'insert' 
                    AND Estado = 'ok' 
                    AND YEAR(Fechahoragrabacion) = @anio 
                    AND MONTH(Fechahoragrabacion) = @mes
                    ORDER BY Fechahoragrabacion";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@anio", anio);
                    command.Parameters.AddWithValue("@mes", mes);
                    command.CommandTimeout = 300;

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int contador = 0;
                            while (await reader.ReadAsync())
                            {
                                var transaccion = new TransaccionData
                                {
                                    Modulo = reader["Modulo"]?.ToString() ?? "",
                                    Mensaje = reader["Mensaje_numero_cd"]?.ToString() ?? "",
                                    Parametros = reader["ParametrosAdicionales_Xml"]?.ToString() ?? "",
                                    FechaHora = Convert.ToDateTime(reader["Fechahoragrabacion"]),
                                    Origen = "FRONT"
                                };

                                // Procesar inmediatamente la transacción
                                await onTransaccionProcesada(transaccion);
                                contador++;

                                // Pequeña pausa cada 1000 registros para mantener responsividad
                                if (contador % 1000 == 0)
                                {
                                    await Task.Delay(1);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error consultando BD Front {anio}/{mes:00}: {ex.Message}", ex);
                    }
                }
            }
        }

        private async Task ConsultarTransaccionesBackPorMesAsync(
            DatabaseConfig config, int anio, int mes, Func<TransaccionData, Task> onTransaccionProcesada)
        {
            using (var connection = new SqlConnection(config.GetConnectionString()))
            {
                await connection.OpenAsync();

                const string query = @"
                    SELECT Modulo, Mensaje_respuesta_ds, ParametrosAdiconles, FechaHoraGrabacion 
                    FROM FacturaElectronica_Transaccion 
                    WHERE Operacion = 'insert' 
                    AND Estado = 'ok' 
                    AND YEAR(FechaHoraGrabacion) = @anio 
                    AND MONTH(FechaHoraGrabacion) = @mes
                    ORDER BY FechaHoraGrabacion";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@anio", anio);
                    command.Parameters.AddWithValue("@mes", mes);
                    command.CommandTimeout = 300;

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int contador = 0;
                            while (await reader.ReadAsync())
                            {
                                var transaccion = new TransaccionData
                                {
                                    Modulo = reader["Modulo"]?.ToString() ?? "",
                                    Mensaje = reader["Mensaje_respuesta_ds"]?.ToString() ?? "",
                                    Parametros = reader["ParametrosAdiconles"]?.ToString() ?? "",
                                    FechaHora = Convert.ToDateTime(reader["FechaHoraGrabacion"]),
                                    Origen = "BACK"
                                };

                                await onTransaccionProcesada(transaccion);
                                contador++;

                                if (contador % 1000 == 0)
                                {
                                    await Task.Delay(1);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error consultando BD Back {anio}/{mes:00}: {ex.Message}", ex);
                    }
                }
            }
        }

        public async Task<int> ContarTransaccionesFrontAsync(DatabaseConfig config, int anio)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Front incompleta");

            try
            {
                using (var connection = new SqlConnection(config.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    const string query = @"
                        SELECT COUNT(*) 
                        FROM Ho_facturaelectronica_transaccion 
                        WHERE Operacion = 'insert' 
                        AND Estado = 'ok' 
                        AND YEAR(Fechahoragrabacion) = @anio";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@anio", anio);
                        command.CommandTimeout = 60;

                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error contando registros Front: {ex.Message}", ex);
            }
        }

        public async Task<int> ContarTransaccionesBackAsync(DatabaseConfig config, int anio)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Back incompleta");

            try
            {
                using (var connection = new SqlConnection(config.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    const string query = @"
                        SELECT COUNT(*) 
                        FROM FacturaElectronica_Transaccion 
                        WHERE Operacion = 'insert' 
                        AND Estado = 'ok' 
                        AND YEAR(FechaHoraGrabacion) = @anio";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@anio", anio);
                        command.CommandTimeout = 60;

                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error contando registros Back: {ex.Message}", ex);
            }
        }
    }
}