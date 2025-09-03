using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IMigracionService
    {
        Task<MigracionResult> EjecutarMigracionAsync(MigracionConfig config, IProgress<int> progreso, TipoMigracion tMigracion);
    }

    public class MigracionService : IMigracionService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IFileService _fileService;
        private readonly IApiService _apiService;

        public MigracionService(
            IDatabaseService databaseService,
            IFileService fileService,
            IApiService apiService)
        {
            _databaseService = databaseService;
            _fileService = fileService;
            _apiService = apiService;
        }

        public async Task<MigracionResult> EjecutarMigracionAsync(MigracionConfig config, IProgress<int> progreso, TipoMigracion t)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.NIT))
                throw new ArgumentException("NIT es requerido");

            // Configurar GC para optimizar memoria con grandes volúmenes
            var originalLatency = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.Batch;

            var resultado = new MigracionResult
            {
                FechaInicio = DateTime.Now,
                NIT = config.NIT
            };

            try
            {
                progreso?.Report(10);

                // Crear directorio de trabajo
                resultado.DirectorioArchivos = _fileService.CrearDirectorioMigracion(config.RutaDescarga, config.NIT);

                // Determinar años a procesar
                var aniosAProcesar = config.TodosLosAnios
                    ? GenerarAniosDisponibles()
                    : config.AniosSeleccionados;

                if (aniosAProcesar == null || aniosAProcesar.Length == 0)
                {
                    throw new InvalidOperationException("No hay años seleccionados para procesar");
                }

                // Determinar tipo de migración desde la config
                var tipoMigracion = DeterminarTipoMigracion(config, t);
                progreso?.Report(20);

                // Procesar cada año con streaming optimizado
                var archivosGenerados = new List<string>();
                var progresoPorAnio = 60 / aniosAProcesar.Length;

                foreach (var anio in aniosAProcesar)
                {
                    try
                    {
                        var archivosAnio = await ProcesarAnioSegunTipoAsync(config, anio, tipoMigracion, resultado.DirectorioArchivos, progreso);
                        if (archivosAnio != null && archivosAnio.Count > 0)
                        {
                            archivosGenerados.AddRange(archivosAnio);
                            resultado.AniosProcesados.Add(anio);
                        }

                        progreso?.Report(20 + (resultado.AniosProcesados.Count * progresoPorAnio));

                        // Limpiar memoria entre años
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        var errorInfo = new ErrorInfo
                        {
                            Anio = anio,
                            Mensaje = ex.Message,
                            Fecha = DateTime.Now
                        };

                        resultado.Errores.Add(errorInfo);

                        _fileService.CrearArchivoLog(
                            resultado.DirectorioArchivos,
                            $"ERROR_{config.NIT}_{anio}",
                            $"Error procesando año {anio}: {ex.Message}",
                            true);
                    }
                }

                resultado.ArchivosGenerados = archivosGenerados;
                progreso?.Report(85);

                // Enviar archivos a API si hay archivos generados
                if (archivosGenerados.Count > 0 && !string.IsNullOrWhiteSpace(config.DatabaseBack?.Password))
                {
                    try
                    {
                        await _apiService.EnviarArchivosAsync(archivosGenerados, config.NIT, config.DatabaseBack.Password);
                        resultado.ArchivosEnviadosAPI = archivosGenerados.Count;
                    }
                    catch (Exception ex)
                    {
                        resultado.Errores.Add(new ErrorInfo
                        {
                            Mensaje = $"Error enviando archivos a API: {ex.Message}",
                            Fecha = DateTime.Now
                        });
                    }
                }

                progreso?.Report(100);
                resultado.FechaFin = DateTime.Now;
                resultado.Exitoso = true;

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.FechaFin = DateTime.Now;
                resultado.Exitoso = false;
                resultado.Errores.Add(new ErrorInfo
                {
                    Mensaje = ex.Message,
                    Fecha = DateTime.Now
                });

                throw;
            }
            finally
            {
                // Restaurar configuración original del GC
                GCSettings.LatencyMode = originalLatency;
            }
        }

        private TipoMigracion DeterminarTipoMigracion(MigracionConfig config, TipoMigracion tipoMigracion)
        {
            bool frontCompleto = config.DatabaseFront.EstaCompleto();
            bool backCompleto = config.DatabaseBack.EstaCompleto();

            if (tipoMigracion == TipoMigracion.NULL)
                throw new InvalidOperationException("No hay configuración válida para ninguna base de datos");

            return tipoMigracion;
        }

        private async Task<List<string>> ProcesarAnioSegunTipoAsync(
            MigracionConfig config, int anio, TipoMigracion tipo, string directorioDestino, IProgress<int> progreso)
        {
            var archivosGenerados = new List<string>();

            switch (tipo)
            {
                case TipoMigracion.Front:
                    var archivoFront = await ProcesarAnioFrontAsync(config, anio, directorioDestino, progreso);
                    if (!string.IsNullOrEmpty(archivoFront))
                        archivosGenerados.Add(archivoFront);
                    break;

                case TipoMigracion.Back:
                    var archivoBack = await ProcesarAnioBackAsync(config, anio, directorioDestino, progreso);
                    if (!string.IsNullOrEmpty(archivoBack))
                        archivosGenerados.Add(archivoBack);
                    break;

                case TipoMigracion.Ambos:
                    var archivoFrontAmbos = await ProcesarAnioFrontAsync(config, anio, directorioDestino, progreso);
                    if (!string.IsNullOrEmpty(archivoFrontAmbos))
                        archivosGenerados.Add(archivoFrontAmbos);

                    var archivoBackAmbos = await ProcesarAnioBackAsync(config, anio, directorioDestino, progreso);
                    if (!string.IsNullOrEmpty(archivoBackAmbos))
                        archivosGenerados.Add(archivoBackAmbos);
                    break;
            }

            return archivosGenerados;
        }

        private async Task<string> ProcesarAnioFrontAsync(MigracionConfig config, int anio, string directorioDestino, IProgress<int> progreso)
        {
            if (!config.DatabaseFront.EstaCompleto())
                return null;

            var nombreArchivo = $"FR_{config.NIT}_{anio}.txt";
            var rutaCompleta = Path.Combine(directorioDestino, nombreArchivo);

            int totalProcesados = 0;
            bool hayDatos = false;

            using (var writer = new StreamWriter(rutaCompleta, false, Encoding.UTF8, bufferSize: 65536))
            {
                await writer.WriteLineAsync("modulo|ldf|parametros|fecha");

                await _databaseService.ConsultarTransaccionesFrontAsync(config.DatabaseFront, anio, async (transaccion) =>
                {
                    await writer.WriteLineAsync(FormatearTransaccion(transaccion));
                    totalProcesados++;
                    hayDatos = true;

                    if (totalProcesados % 5000 == 0)
                    {
                        await writer.FlushAsync();
                    }
                });

                await writer.FlushAsync();
            }

            if (!hayDatos || totalProcesados == 0)
            {
                if (File.Exists(rutaCompleta))
                    File.Delete(rutaCompleta);
                return null;
            }

            _fileService.CrearArchivoLog(
                directorioDestino,
                $"RESUMEN_FR_{config.NIT}_{anio}",
                $"Año {anio} FRONT procesado exitosamente\n" +
                $"Total registros: {totalProcesados:N0}\n" +
                $"Archivo: {nombreArchivo}\n" +
                $"Tamaño: {new FileInfo(rutaCompleta).Length / 1024:N0} KB"
            );

            return rutaCompleta;
        }

        private async Task<string> ProcesarAnioBackAsync(MigracionConfig config, int anio, string directorioDestino, IProgress<int> progreso)
        {
            if (!config.DatabaseBack.EstaCompleto())
                return null;

            var nombreArchivo = $"BA_{config.NIT}_{anio}.txt";
            var rutaCompleta = Path.Combine(directorioDestino, nombreArchivo);

            int totalProcesados = 0;
            bool hayDatos = false;

            using (var writer = new StreamWriter(rutaCompleta, false, Encoding.UTF8, bufferSize: 65536))
            {
                await writer.WriteLineAsync("modulo|ldf|parametros|fecha");

                await _databaseService.ConsultarTransaccionesBackAsync(config.DatabaseBack, anio, async (transaccion) =>
                {
                    await writer.WriteLineAsync(FormatearTransaccion(transaccion));
                    totalProcesados++;
                    hayDatos = true;

                    if (totalProcesados % 5000 == 0)
                    {
                        await writer.FlushAsync();
                    }
                });

                await writer.FlushAsync();
            }

            if (!hayDatos || totalProcesados == 0)
            {
                if (File.Exists(rutaCompleta))
                    File.Delete(rutaCompleta);
                return null;
            }

            _fileService.CrearArchivoLog(
                directorioDestino,
                $"RESUMEN_BA_{config.NIT}_{anio}",
                $"Año {anio} BACK procesado exitosamente\n" +
                $"Total registros: {totalProcesados:N0}\n" +
                $"Archivo: {nombreArchivo}\n" +
                $"Tamaño: {new FileInfo(rutaCompleta).Length / 1024:N0} KB"
            );

            return rutaCompleta;
        }

        private static string FormatearTransaccion(TransaccionData transaccion)
        {
            return $"{LimpiarTexto(transaccion.Modulo)}|" +
                   $"{LimpiarTexto(transaccion.Mensaje)}|" +
                   $"{LimpiarTexto(transaccion.Parametros)}|" +
                   $"{transaccion.FechaHora:yyyy-MM-dd HH:mm:ss}";
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return texto.Replace("\r", "")
                       .Replace("\n", "")
                       .Replace("\t", "")
                       .Replace("|", "") // Quitar pipes para evitar problemas con el formato
                       .Trim();
        }

        private static int[] GenerarAniosDisponibles()
        {
            var anioActual = DateTime.Now.Year;
            var anios = new int[7];
            for (int i = 0; i < 7; i++)
            {
                anios[i] = anioActual - i;
            }
            return anios;
        }
    }

    public class MigracionResult
    {
        public bool Exitoso { get; set; }
        public string NIT { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string DirectorioArchivos { get; set; }
        public List<string> ArchivosGenerados { get; set; }
        public List<int> AniosProcesados { get; set; }
        public List<ErrorInfo> Errores { get; set; }
        public int ArchivosEnviadosAPI { get; set; }

        public MigracionResult()
        {
            ArchivosGenerados = new List<string>();
            AniosProcesados = new List<int>();
            Errores = new List<ErrorInfo>();
        }

        public TimeSpan DuracionTotal => FechaFin - FechaInicio;
    }

    public class ErrorInfo
    {
        public int? Anio { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
    }
}