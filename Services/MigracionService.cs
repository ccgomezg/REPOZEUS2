using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IMigracionService
    {
        Task<MigracionResult> EjecutarMigracionAsync(
            MigracionConfig config,
            IProgress<int> progreso,
            TipoMigracion tipoMigracion,
            string rutaCompleta,
            bool ejecutarSp);
    }

    public class MigracionService : IMigracionService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IFileService _fileService;
        private readonly IApiService _apiService;
        private readonly IMigracionLogService _logService;
        private readonly IConfigService _configService;
        private readonly long _maxZipSizeBytes;
        private readonly bool _habilitarDivisionZip;

        private const int BUFFER_SIZE = 65536;
        private const int BATCH_SIZE = 5000;
        private const string CONFIG_FILE = "config.txt";
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        public MigracionService(
            IDatabaseService databaseService,
            IFileService fileService,
            IApiService apiService,
            IConfigService configService,
            IMigracionLogService logService = null,
            bool habilitarDivisionZip = true,
            double maxZipSizeMB = 0.1)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logService = logService ?? new MigracionLogService();
            _habilitarDivisionZip = habilitarDivisionZip;
            _maxZipSizeBytes = (long)maxZipSizeMB;
        }

        public async Task<MigracionResult> EjecutarMigracionAsync(
            MigracionConfig config,
            IProgress<int> progreso,
            TipoMigracion tipoMigracion,
            string rutaCompleta,
            bool ejecutarSp)
        {
            ValidarParametros(config, tipoMigracion);

            var configPath = GetConfigPath();
            var resultado = InicializarResultado(config, rutaCompleta);

            GCSettings.LatencyMode = GCLatencyMode.Batch;

            try
            {
                progreso?.Report(10);

                var aniosPendientes = ObtenerAniosPendientes(config, tipoMigracion, resultado);

                if (aniosPendientes.Length == 0)
                {
                    await ProcesarAniosCompletados(config, tipoMigracion, resultado, configPath, ejecutarSp);
                    return resultado;
                }

                progreso?.Report(20);
                await ProcesarAnios(config, tipoMigracion, aniosPendientes, resultado, progreso);

                // Procesar años pendientes de notificar
                await NotificarAniosPendientes(config, tipoMigracion, resultado);

                // Ejecutar SP si corresponde
                if (ejecutarSp)
                {
                    await EjecutarStoredProcedure(config, tipoMigracion, configPath);
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
                GCSettings.LatencyMode = GCLatencyMode.Interactive;
            }
        }

        private static string GetConfigPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_FILE);
        }

        private static MigracionResult InicializarResultado(MigracionConfig config, string rutaCompleta)
        {
            return new MigracionResult
            {
                FechaInicio = DateTime.Now,
                NIT = config.NIT,
                DirectorioArchivos = rutaCompleta
            };
        }

        private void ValidarParametros(MigracionConfig config, TipoMigracion tipoMigracion)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.NIT))
                throw new ArgumentException("NIT es requerido");

            if (tipoMigracion == TipoMigracion.NULL)
                throw new InvalidOperationException("Debe seleccionar un tipo de migración válido");
        }

        private async Task ProcesarAniosCompletados(
            MigracionConfig config,
            TipoMigracion tipoMigracion,
            MigracionResult resultado,
            string configPath,
            bool ejecutarSp)
        {
            resultado.MensajeRecuperacion = "Todos los años ya procesados.";
            resultado.Exitoso = true;
            resultado.FechaFin = DateTime.Now;

            await NotificarAniosPendientes(config, tipoMigracion, resultado);

            if (ejecutarSp)
            {
                await EjecutarStoredProcedure(config, tipoMigracion, configPath);
            }
        }

        private async Task NotificarAniosPendientes(
            MigracionConfig config,
            TipoMigracion tipoMigracion,
            MigracionResult resultado)
        {
            var cortesSinNotificar = ObtenerAniosPendientesNotificar(config, tipoMigracion, resultado);
            int archivos_enviados = 0;

            foreach (var item in cortesSinNotificar)
            {
               archivos_enviados += await EnviarArchivoAS3(config.NIT, item.año, item.ruta, tipoMigracion, item.CantidadLdf,item.ticket);
            }

            if (archivos_enviados > resultado.ArchivosEnviadosAPI )
            { 
                 resultado.ArchivosEnviadosAPI = archivos_enviados;
            }
        }

        private async Task<int> EnviarArchivoAS3(string nit, string año, string ruta, TipoMigracion tipoMigracion, int totalLdf, string ticket)
        {
            var fileName = Path.GetFileName(ruta);
            var httpAnswer = await _apiService.EnviarArchivoZipAsync(ruta, nit, ticket, totalLdf.ToString());

            bool exitoso = httpAnswer.StatusCode == 200;
            string mensaje = $"{httpAnswer.StatusCode} - {httpAnswer.Content}";

            _logService.ActualizarSubidoS3(nit, int.Parse(año), tipoMigracion, exitoso, mensaje, ruta);
            if (exitoso)
                return 1;
            else return 0;
        }

        private async Task EjecutarStoredProcedure(
            MigracionConfig config,
            TipoMigracion tipoMigracion,
            string configPath)
        {
            DatabaseConfig database;
            string spConfigKey;
            bool spEjecutado;

            if (tipoMigracion == TipoMigracion.Back)
            {
                database = config.DatabaseBack;
                spConfigKey = "SpBackEjecutado";
                spEjecutado = config.DatabaseBack.SpEjecutado;
            }
            else
            {
                database = config.DatabaseFront;
                spConfigKey = "SpFrontEjecutado";
                spEjecutado = config.DatabaseFront.SpEjecutado;
            }

            if (spEjecutado) return;

            string resultado = await _databaseService.ConfigSp(database);

            if (resultado == "OK")
            {
                _configService.ActualizarValorConfig(configPath, spConfigKey, "true");
                _logService.RegistrarAnioCompletado("", 0, tipoMigracion, 0, "OK","");
            }
            else
            {
                _logService.RegistrarAnioCompletado("", 0, tipoMigracion, 0, resultado, "");
                MostrarMensajeError(resultado);
            }
        }

        private static void MostrarMensajeError(string mensaje)
        {
            MessageBox.Show(
                mensaje,
                "Aviso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private int[] ObtenerAniosPendientes(
            MigracionConfig config,
            TipoMigracion tipo,
            MigracionResult resultado)
        {
            var aniosSeleccionados = ObtenerAniosSeleccionados(config);
            ValidarAniosSeleccionados(aniosSeleccionados);

            var aniosCompletados = _logService.ObtenerAniosCompletados(config.NIT, tipo);
            var aniosPendientes = aniosSeleccionados.Where(a => !aniosCompletados.Contains(a)).ToArray();

            if (aniosCompletados.Count > 0)
            {
                resultado.MensajeRecuperacion =
                    $"Saltados {aniosCompletados.Count} años. Procesando {aniosPendientes.Length} pendientes.";
            }

            return aniosPendientes;
        }

        private List<(string año, string ruta, int CantidadLdf, string ticket)> ObtenerAniosPendientesNotificar(
            MigracionConfig config,
            TipoMigracion tipo,
            MigracionResult resultado)
        {
            var aniosSeleccionados = ObtenerAniosSeleccionados(config);
            ValidarAniosSeleccionados(aniosSeleccionados);

            var seleccionados = new HashSet<int>(aniosSeleccionados);
            var aniosErrorNotificar = _logService.ObtenerAniosErrorNotificar(config.NIT, tipo,0);

            var aniosSinNotificar = aniosErrorNotificar
                .Where(t => int.TryParse(t.año, out var anio) && seleccionados.Contains(anio))
                .ToList();

            if (aniosSinNotificar.Count > 0)
            {
                resultado.MensajeRecuperacion = $"Procesando {aniosSinNotificar.Count} pendientes.";
            }

            return aniosSinNotificar;
        }

        private int[] ObtenerAniosSeleccionados(MigracionConfig config)
        {
            return config.TodosLosAnios ? GenerarAniosDisponibles() : config.AniosSeleccionados;
        }

        private static void ValidarAniosSeleccionados(int[] aniosSeleccionados)
        {
            if (aniosSeleccionados == null || aniosSeleccionados.Length == 0)
                throw new InvalidOperationException("No hay años seleccionados");
        }

        private async Task ProcesarAnios(
            MigracionConfig config,
            TipoMigracion tipoMigracion,
            int[] aniosPendientes,
            MigracionResult resultado,
            IProgress<int> progreso)
        {
            var progresoPorAnio = 60 / aniosPendientes.Length;
            int totalGenerados = 0;
            int totalSubidos = 0;

            for (int i = 0; i < aniosPendientes.Length; i++)
            {
                var anio = aniosPendientes[i];

                try
                {
                    var procesadoResult = await ProcesarAnio(
                        config, anio, tipoMigracion, resultado);

                    totalGenerados += procesadoResult.generados;
                    totalSubidos += procesadoResult.subidos;
                    resultado.AniosProcesados.Add(anio);
                }
                catch (Exception ex)
                {
                    RegistrarError(resultado, anio, ex);
                }

                ReportarProgreso(progreso, i, progresoPorAnio);
                GC.Collect();
            }

            resultado.ArchivosGenerados = totalGenerados.ToString();
            resultado.ArchivosEnviadosAPI = totalSubidos;
        }

        private async Task<(int generados, int subidos)> ProcesarAnio(
            MigracionConfig config,
            int anio,
            TipoMigracion tipoMigracion,
            MigracionResult resultado)
        {
            var archivoAnio = await GenerarArchivoAnio(
                config, anio, tipoMigracion, resultado.DirectorioArchivos);

            if (string.IsNullOrEmpty(archivoAnio))
                return (0, 0);

            var archivosEnviados = await DividirYEnviarArchivos(
                archivoAnio,
                config.NIT,
                anio,
                tipoMigracion,
                config.DividirZip,
                config.tamanioZip, config);

            return (archivosEnviados.Count, archivosEnviados.Count(a => a.subido));
        }

        private void RegistrarError(MigracionResult resultado, int anio, Exception ex)
        {
            resultado.Errores.Add(new ErrorInfo
            {
                Anio = anio,
                Mensaje = ex.Message,
                Fecha = DateTime.Now
            });

            _fileService.CrearArchivoLog(
                resultado.DirectorioArchivos,
                $"ERROR_{resultado.NIT}_{anio}",
                $"Error procesando año {anio}: {ex.Message}",
                true);
        }

        private static void ReportarProgreso(IProgress<int> progreso, int indice, int progresoPorAnio)
        {
            progreso?.Report(Math.Min(20 + ((indice + 1) * progresoPorAnio), 80));
        }

        private async Task<List<(string archivo, bool subido)>> DividirYEnviarArchivos(
            string rutaArchivo,
            string nit,
            int anio,
            TipoMigracion tipo,
            bool dividirZip,
            long tamanioZip, MigracionConfig config)
        {
            var resultados = new List<(string, bool)>();
            var archivos = _fileService.DividirZip(rutaArchivo, tamanioZip, dividirZip);

            foreach (var archivo in archivos)
            {
                var subidoExitosamente = await EnviarArchivo(archivo, nit, anio, tipo, archivos.Count, config);
                resultados.Add((archivo, subidoExitosamente));
            }

            return resultados;
        }

        private async Task<bool> EnviarArchivo(
            string archivo,
            string nit,
            int anio,
            TipoMigracion tipo,
            int totalArchivos, MigracionConfig config)
        {
            var nombreArchivo = Path.GetFileName(archivo);

            try
            {
                int cantidadLdf = CantidadLdfZip(archivo);
                _logService.RegistrarAnioCompletado(nit, anio, tipo, cantidadLdf, archivo, config.ticket);

                var httpAnswer = await _apiService.EnviarArchivoZipAsync(
                    archivo, nit, config.ticket, cantidadLdf.ToString());

                bool exitoso = httpAnswer.StatusCode == 200;
                string mensaje = $"{httpAnswer.StatusCode} - {httpAnswer.Content}";

                _logService.ActualizarSubidoS3(nit, anio, tipo, exitoso, mensaje, archivo);

                return exitoso;
            }
            catch (Exception ex)
            {
                RegistrarErrorEnvio(archivo, nit, anio, tipo, ex);
                return false;
            }
        }

        private void RegistrarErrorEnvio(
            string archivo,
            string nit,
            int anio,
            TipoMigracion tipo,
            Exception ex)
        {
            _fileService.CrearArchivoLog(
                Path.GetDirectoryName(archivo),
                $"ERROR_ENVIO_{Path.GetFileNameWithoutExtension(archivo)}",
                ex.Message,
                true);

            _logService.ActualizarSubidoS3(nit, anio, tipo, false, ex.ToString(), archivo);
        }

        private async Task<string> GenerarArchivoAnio(
            MigracionConfig config,
            int anio,
            TipoMigracion tipo,
            string directorio)
        {
            switch (tipo)
            {
                case TipoMigracion.Front:
                    return await ProcesarAnioTipo(config, anio, directorio, true);
                case TipoMigracion.Back:
                    return await ProcesarAnioTipo(config, anio, directorio, false);
                default:
                    return null;
            }
        }

        private async Task<string> ProcesarAnioTipo(
            MigracionConfig config,
            int anio,
            string directorio,
            bool esFront)
        {
            var configuracion = ObtenerConfiguracionTipo(config, esFront);

            if (!configuracion.database.EstaCompleto())
                return null;

            var nombreArchivo = GenerarNombreArchivo(configuracion.prefijo, config.NIT, anio);
            var rutaCompleta = Path.Combine(directorio, nombreArchivo);

            var estadisticas = await EscribirTransacciones(
                rutaCompleta, configuracion.database, anio, esFront);

            if (!estadisticas.hayDatos)
            {
                File.Delete(rutaCompleta);
                return null;
            }

            rutaCompleta = ComprimirArchivo(rutaCompleta);
            GenerarLogResumen(directorio, configuracion.prefijo, config.NIT, anio,
                estadisticas.totalProcesados, rutaCompleta);

            return rutaCompleta;
        }

        private (DatabaseConfig database, string prefijo) ObtenerConfiguracionTipo(
            MigracionConfig config,
            bool esFront)
        {
            if (esFront)
                return (config.DatabaseFront, "FR");
            else
                return (config.DatabaseBack, "BA");
        }

        private static string GenerarNombreArchivo(string prefijo, string nit, int anio)
        {
            string codigo = DateTime.Now.ToString("yyyyMMddHHmmss");
            return $"{prefijo}_{nit}_{anio}_{codigo}.txt";
        }

        private async Task<(int totalProcesados, bool hayDatos)> EscribirTransacciones(
            string rutaArchivo,
            DatabaseConfig database,
            int anio,
            bool esFront)
        {
            int totalProcesados = 0;
            bool hayDatos = false;

            using (var writer = new StreamWriter(rutaArchivo, false, Encoding.UTF8, bufferSize: BUFFER_SIZE))
            {
                //await writer.WriteLineAsync("modulo|ldf|parametros|fecha");

                async Task ProcesarTransaccion(TransaccionData transaccion)
                {
                    await writer.WriteLineAsync(FormatearTransaccion(transaccion));
                    totalProcesados++;
                    hayDatos = true;

                    if (totalProcesados % BATCH_SIZE == 0)
                        await writer.FlushAsync();
                }

                if (esFront)
                    await _databaseService.ConsultarTransaccionesFrontAsync(database, anio, ProcesarTransaccion);
                else
                    await _databaseService.ConsultarTransaccionesBackAsync(database, anio, ProcesarTransaccion);

                await writer.FlushAsync();
            }

            return (totalProcesados, hayDatos);
        }

        private string ComprimirArchivo(string rutaArchivo)
        {
            var rutaZip = _fileService.CrearArchivoZip(rutaArchivo);
            if (!string.IsNullOrEmpty(rutaZip))
            {
                File.Delete(rutaArchivo);
                return rutaZip;
            }
            return rutaArchivo;
        }

        private void GenerarLogResumen(
            string directorio,
            string prefijo,
            string nit,
            int anio,
            int totalProcesados,
            string rutaArchivo)
        {
            var tamanioKB = new FileInfo(rutaArchivo).Length / 1024;
            var contenido = $"Año {anio} {prefijo} procesado\n" +
                          $"Registros: {totalProcesados:N0}\n" +
                          $"Archivo: {Path.GetFileName(rutaArchivo)}\n" +
                          $"Tamaño: {tamanioKB:N0} KB";

            _fileService.CrearArchivoLog(
                directorio,
                $"RESUMEN_{prefijo}_{nit}_{anio}",
                contenido);
        }

        private static string FormatearTransaccion(TransaccionData t)
        {
            return $"{LimpiarTexto(t.Modulo)}|{LimpiarTexto(t.Mensaje)}|" +
                   $"{LimpiarTexto(t.Parametros)}|{t.FechaHora:yyyy-MM-dd HH:mm:ss}";
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";

            return texto
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace("|", "")
                .Trim();
        }

       
        private static int[] GenerarAniosDisponibles()
        {
            var anioActual = DateTime.Now.Year;
            return Enumerable.Range(0, 9).Select(i => anioActual - i).ToArray();
        }

        private static int CantidadLdfZip(string ruta)
        {
            try
            {
                int lineasTxt = 0;
                using (var archive = ZipFile.OpenRead(ruta))
                {
                    var archivoTxt = archive.Entries.FirstOrDefault(e =>
                        e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

                    if (archivoTxt != null)
                    {
                        using (var stream = archivoTxt.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            while (reader.ReadLine() != null)
                                lineasTxt++;
                        }
                    }
                }

                return lineasTxt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;

            }
        }

    }
}