using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Models;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Services
{
    public interface IDatabaseService
    {
        Task<(bool Ok, string Mensaje)> VerificarConexionAsync(DatabaseConfig config, int timeoutSeg = 5, System.Threading.CancellationToken cancellationToken = default);
        Task ConsultarTransaccionesFrontAsync(DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada);
        Task ConsultarTransaccionesBackAsync(DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada);
        Task<int> ContarTransaccionesFrontAsync(DatabaseConfig config, int anio);
        Task<int> ContarTransaccionesBackAsync(DatabaseConfig config, int anio);
        Task<string> ConfigSp(DatabaseConfig config);

    }

    public class ColumnMapping
    {
        public string modulo { get; set; }
        public string ldf { get; set; }
        public string parametros_Adicionales { get; set; }
        public string fecha { get; set; }
        public string nombre_de_la_tabla_Sql { get; set; }
        public string pk { get; set; } 
    }

    public class DatabaseService : IDatabaseService
    {
        private ColumnMapping _columnasFront;
        private ColumnMapping _columnasBack;

        private const int CMD_TIMEOUT_SEC = 0;
        private const bool USE_NOLOCK = true;

        
        private readonly ColumnMapping _columnasFrontDefault = new ColumnMapping
        {
        
        // ---------- Defaults BACK ----------
            modulo = "Modulo",
            ldf = "Mensaje_numero_cd",
            parametros_Adicionales = "ParametrosAdicionales_Xml",
            fecha = "Fechahoragrabacion",
            nombre_de_la_tabla_Sql = "Ho_facturaelectronica_transaccion",
            pk = null
        };

        // ---------- Defaults BACK ----------
        private readonly ColumnMapping _columnasBackDefault = new ColumnMapping
        {
            modulo = "Modulo",
            ldf = "Mensaje_respuesta_ds",
            parametros_Adicionales = "ParametrosAdiconles",
            fecha = "FechaHoraGrabacion",
            nombre_de_la_tabla_Sql = "FacturaElectronica_Transaccion",
            pk = null
        };

        public DatabaseService()
        {
            CargarConfiguracionColumnas();
        }

        private static DateTime InicioDeMes(int anio, int mes) => new DateTime(anio, mes, 1, 0, 0, 0);
        private static DateTime FinExclusivoDeMes(int anio, int mes) => (mes == 12) ? new DateTime(anio + 1, 1, 1) : new DateTime(anio, mes + 1, 1);
        private static DateTime InicioDeAnio(int anio) => new DateTime(anio, 1, 1);
        private static DateTime InicioDeAnioSiguiente(int anio) => new DateTime(anio + 1, 1, 1);

        private void CargarConfiguracionColumnas()
        {
            try
            {
                _columnasFront = CloneMapping(_columnasFrontDefault);
                _columnasBack = CloneMapping(_columnasBackDefault);

                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
                if (!File.Exists(configPath)) return;

                var lineas = File.ReadAllLines(configPath, Encoding.UTF8);
                for (int i = 0; i < lineas.Length; i++)
                {
                    var raw = lineas[i];
                    if (string.IsNullOrWhiteSpace(raw)) continue;

                    var line = raw.Trim();
                    if (line.StartsWith("#")) continue;

                    if (!TrySplitKeyValueEqualsOnly(line, out var clave, out var valor)) continue;

                    try
                    {
                        if (clave.Equals("SELECT_FRONT", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(valor))
                        {
                            var mapping = JsonConvert.DeserializeObject<ColumnMapping>(valor);
                            if (mapping != null) ActualizarMapping(_columnasFront, mapping);
                        }
                        else if (clave.Equals("SELECT_BACK", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(valor))
                        {
                            var mapping = JsonConvert.DeserializeObject<ColumnMapping>(valor);
                            if (mapping != null) ActualizarMapping(_columnasBack, mapping);
                        }
                    }
                    catch (JsonException)
                    {
                     
                    }
                }
            }
            catch
            {
                _columnasFront = CloneMapping(_columnasFrontDefault);
                _columnasBack = CloneMapping(_columnasBackDefault);
            }
        }

        private static bool TrySplitKeyValueEqualsOnly(string line, out string key, out string value)
        {
            key = null; value = null;
            int idx = line.IndexOf('=');
            if (idx <= 0) return false;

            key = line.Substring(0, idx).Trim();
            value = line.Substring(idx + 1).Trim();

            
            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                value = value.Substring(1, value.Length - 2);

            return !string.IsNullOrWhiteSpace(key);
        }

        private ColumnMapping CloneMapping(ColumnMapping o) => new ColumnMapping
        {
            modulo = o.modulo,
            ldf = o.ldf,
            parametros_Adicionales = o.parametros_Adicionales,
            fecha = o.fecha,
            nombre_de_la_tabla_Sql = o.nombre_de_la_tabla_Sql,
            pk = o.pk
        };

        private void ActualizarMapping(ColumnMapping target, ColumnMapping src)
        {
            if (!string.IsNullOrWhiteSpace(src.modulo)) target.modulo = src.modulo;
            if (!string.IsNullOrWhiteSpace(src.ldf)) target.ldf = src.ldf;
            if (!string.IsNullOrWhiteSpace(src.parametros_Adicionales)) target.parametros_Adicionales = src.parametros_Adicionales;
            if (!string.IsNullOrWhiteSpace(src.fecha)) target.fecha = src.fecha;
            if (!string.IsNullOrWhiteSpace(src.nombre_de_la_tabla_Sql)) target.nombre_de_la_tabla_Sql = src.nombre_de_la_tabla_Sql;
            if (!string.IsNullOrWhiteSpace(src.pk)) target.pk = src.pk;
        }

        private string BuildSelectStreaming(ColumnMapping m)
        {
            var nolock = USE_NOLOCK ? " WITH (NOLOCK)" : "";

       
            if (!string.IsNullOrWhiteSpace(m.pk))
            {
                return $@"
                    SELECT {m.modulo}, {m.ldf}, {m.parametros_Adicionales}, {m.fecha}, {m.pk}
                    FROM {m.nombre_de_la_tabla_Sql}{nolock}
                    WHERE Operacion = 'insert'
                      AND Estado = 'ok'
                      AND {m.fecha} >= @ini AND {m.fecha} < @fin
                    OPTION (FAST 1000);";
            }
            else
            {
                return $@"
                    SELECT {m.modulo}, {m.ldf}, {m.parametros_Adicionales}, {m.fecha}
                    FROM {m.nombre_de_la_tabla_Sql}{nolock}
                    WHERE Operacion = 'insert'
                      AND Estado = 'ok'
                      AND {m.fecha} >= @ini AND {m.fecha} < @fin
                    OPTION (FAST 1000);";
            }
        }

        private string BuildCount(ColumnMapping m) => $@"
            SELECT COUNT(*)
            FROM {m.nombre_de_la_tabla_Sql}
            WHERE Operacion = 'insert' AND Estado = 'ok'
              AND {m.fecha} >= @ini AND {m.fecha} < @fin;";

        public async Task<(bool Ok, string Mensaje)> VerificarConexionAsync(
            DatabaseConfig config, int timeoutSeg = 5, System.Threading.CancellationToken cancellationToken = default)
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
                using (var command = new SqlCommand("SELECT 1", connection))
                {
                    await connection.OpenAsync(cancellationToken);
                    var result = await command.ExecuteScalarAsync(cancellationToken);
                    return (Convert.ToInt32(result) == 1, "Conexión exitosa");
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

        public async Task ConsultarTransaccionesFrontAsync(DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Front incompleta");

            CargarConfiguracionColumnas();
            for (int mes = 1; mes <= 12; mes++)
                await ConsultarTransaccionesPorMesStreamingAsync(config, _columnasFront, anio, mes, onTransaccionProcesada, "FRONT");
        }

        public async Task ConsultarTransaccionesBackAsync(DatabaseConfig config, int anio, Func<TransaccionData, Task> onTransaccionProcesada)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Back incompleta");

            CargarConfiguracionColumnas();
            for (int mes = 1; mes <= 12; mes++)
                await ConsultarTransaccionesPorMesStreamingAsync(config, _columnasBack, anio, mes, onTransaccionProcesada, "BACK");
        }

        private static string SafeGetString(SqlDataReader r, int ord)
        {
            if (r.IsDBNull(ord)) return "";
            object o = r.GetValue(ord);
            return o?.ToString() ?? "";
        }

        private static DateTime SafeGetDateTime(SqlDataReader r, int ord)
        {
            if (r.IsDBNull(ord)) return DateTime.MinValue;
            object o = r.GetValue(ord);
            return Convert.ToDateTime(o);
        }

        private static long? SafeGetInt64Nullable(SqlDataReader r, int ord)
        {
            if (r.IsDBNull(ord)) return null;
            object o = r.GetValue(ord);
            try { return Convert.ToInt64(o); } catch { return null; }
        }

        private async Task ConsultarTransaccionesPorMesStreamingAsync(
             DatabaseConfig config, ColumnMapping m, int anio, int mes,
             Func<TransaccionData, Task> onRow, string origen)
        {
            var ini = new DateTime(anio, mes, 1, 0, 0, 0);
            var fin = (mes == 12) ? new DateTime(anio + 1, 1, 1) : new DateTime(anio, mes + 1, 1);

            string sql = BuildSelectStreaming(m);
            bool hasPk = !string.IsNullOrWhiteSpace(m.pk);

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(config.GetConnectionString());
                await conn.OpenAsync().ConfigureAwait(false);

                using (var pre = new SqlCommand("SET NOCOUNT ON; SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", conn))
                {
                    pre.CommandTimeout = 5;
                    await pre.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = CMD_TIMEOUT_SEC;
                    cmd.Parameters.Add("@ini", SqlDbType.DateTime2).Value = ini;
                    cmd.Parameters.Add("@fin", SqlDbType.DateTime2).Value = fin;

                    using (var reader = await cmd.ExecuteReaderAsync(
                        CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.CloseConnection
                    ).ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            string modulo = SafeGetString(reader, 0);
                            string mensaje = SafeGetString(reader, 1);
                            string parametros = SafeGetString(reader, 2);
                            DateTime fecha = SafeGetDateTime(reader, 3);

                            var t = new TransaccionData
                            {
                                Modulo = modulo,
                                Mensaje = mensaje,
                                Parametros = parametros,
                                FechaHora = fecha,
                                Origen = origen
                            };

                            await onRow(t).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (conn != null && conn.State != ConnectionState.Closed) conn.Close();
                    if (conn != null) SqlConnection.ClearPool(conn);
                }
                catch { /* noop */ }

                throw new Exception($"Error consultando {origen} {anio}/{mes:00}: {ex.Message}", ex);
            }
            finally
            {
                if (conn != null) conn.Dispose();
            }
        }

        public async Task<int> ContarTransaccionesFrontAsync(DatabaseConfig config, int anio)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Front incompleta");

            CargarConfiguracionColumnas();
            var ini = InicioDeAnio(anio);
            var fin = InicioDeAnioSiguiente(anio);

            using (var conn = new SqlConnection(config.GetConnectionString()))
            using (var cmd = new SqlCommand(BuildCount(_columnasFront), conn))
            {
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ini", SqlDbType.DateTime2).Value = ini;
                cmd.Parameters.Add("@fin", SqlDbType.DateTime2).Value = fin;

                await conn.OpenAsync();
                using (var isoCmd = new SqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", conn))
                {
                    isoCmd.CommandTimeout = 5;
                    await isoCmd.ExecuteNonQueryAsync();
                }

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<int> ContarTransaccionesBackAsync(DatabaseConfig config, int anio)
        {
            if (!config.EstaCompleto())
                throw new ArgumentException("Configuración de base de datos Back incompleta");

            CargarConfiguracionColumnas();
            var ini = InicioDeAnio(anio);
            var fin = InicioDeAnioSiguiente(anio);

            using (var conn = new SqlConnection(config.GetConnectionString()))
            using (var cmd = new SqlCommand(BuildCount(_columnasBack), conn))
            {
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ini", SqlDbType.DateTime2).Value = ini;
                cmd.Parameters.Add("@fin", SqlDbType.DateTime2).Value = fin;

                await conn.OpenAsync();
                using (var isoCmd = new SqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", conn))
                {
                    isoCmd.CommandTimeout = 5;
                    await isoCmd.ExecuteNonQueryAsync();
                }

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<string> ConfigSp(DatabaseConfig config)
        {
            try {
                if (config == null || !config.EstaCompleto() || config.SpName == null || config.SpName == "")
                    throw new ArgumentException("Configuración de base de datos incompleta");

                string nombreSp = config.SpName;

                using (var conn = new SqlConnection(config.GetConnectionString()))
                using (var cmd = new SqlCommand(nombreSp, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CMD_TIMEOUT_SEC;

                    await conn.OpenAsync().ConfigureAwait(false);
                    int filas = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return "OK";
                }
            }
            catch(Exception ex) {


                return ex.ToString();
            }
            
        }
    }
}