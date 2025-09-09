using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
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
            TipoMigracion tMigracion,
            string rutaCompleta);


    }

    public class MigracionService : IMigracionService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IFileService _fileService;
        private readonly IApiService _apiService;
        private readonly IMigracionLogService _logService;

        // Configuración de división
        private readonly bool _habilitarDivisionZip;
        private readonly long _maxZipSizeBytes;

        public MigracionService(
            IDatabaseService databaseService,
            IFileService fileService,
            IApiService apiService,
            IMigracionLogService logService = null,
            bool habilitarDivisionZip = true,
            double maxZipSizeMB =50)
        {
            _databaseService = databaseService;
            _fileService = fileService;
            _apiService = apiService;
            _logService = logService ?? new MigracionLogService();
            _habilitarDivisionZip = habilitarDivisionZip;
            _maxZipSizeBytes = (long)(maxZipSizeMB * 1024 * 1024);
        }

        public async Task<MigracionResult> EjecutarMigracionAsync(
            MigracionConfig config,
            IProgress<int> progreso,
            TipoMigracion tipoMigracion,
            string rutaCompleta)
        {
            ValidarParametros(config, tipoMigracion);

            var resultado = new MigracionResult
            {
                FechaInicio = DateTime.Now,
                NIT = config.NIT,
                DirectorioArchivos = rutaCompleta
            };

            GCSettings.LatencyMode = GCLatencyMode.Batch;

            try
            {
                progreso?.Report(10);

                var aniosPendientes = ObtenerAniosPendientes(config, tipoMigracion, resultado);
                if (aniosPendientes.Length == 0)
                {
                    resultado.MensajeRecuperacion = "Todos los años ya procesados.";
                    resultado.Exitoso = true;
                    resultado.FechaFin = DateTime.Now;

                    if (tipoMigracion == TipoMigracion.Back && !config.DatabaseBack.SpEjecutado)
                    {
                        int spEjecutado = await _databaseService.ConfigSp(config.DatabaseBack);


                    }
                    else if (tipoMigracion == TipoMigracion.Front && !config.DatabaseFront.SpEjecutado)
                    {
                        int spEjecutado = await _databaseService.ConfigSp(config.DatabaseFront);
                    }

                    return resultado;
                }

                progreso?.Report(20);

                await ProcesarAnios(config, tipoMigracion, aniosPendientes, resultado, progreso);

                progreso?.Report(100);
                resultado.FechaFin = DateTime.Now;
                resultado.Exitoso = true;


                if(tipoMigracion == TipoMigracion.Back && !config.DatabaseBack.SpEjecutado && config.DatabaseBack.SpEjecutado == null )
                {
                    int spEjecutado = await _databaseService.ConfigSp(config.DatabaseBack);
                   

                }
                else if(tipoMigracion == TipoMigracion.Front && !config.DatabaseFront.SpEjecutado)
                {
                    int spEjecutado = await _databaseService.ConfigSp(config.DatabaseFront);
                }


                return resultado;
            }
            catch (Exception ex)
            {
                resultado.FechaFin = DateTime.Now;
                resultado.Exitoso = false;
                resultado.Errores.Add(new ErrorInfo { Mensaje = ex.Message, Fecha = DateTime.Now });
                throw;
            }
            finally
            {
                GCSettings.LatencyMode = GCLatencyMode.Interactive;
            }
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

        private int[] ObtenerAniosPendientes(MigracionConfig config, TipoMigracion tipo, MigracionResult resultado)
        {
            var aniosSeleccionados = config.TodosLosAnios
                ? GenerarAniosDisponibles()
                : config.AniosSeleccionados;

            if (aniosSeleccionados == null || aniosSeleccionados.Length == 0)
                throw new InvalidOperationException("No hay años seleccionados");

            var aniosCompletados = _logService.ObtenerAniosCompletados(config.NIT, tipo);
            var aniosPendientes = aniosSeleccionados.Where(a => !aniosCompletados.Contains(a)).ToArray();

            if (aniosCompletados.Count > 0)
            {
                resultado.MensajeRecuperacion =
                    $"Saltados {aniosCompletados.Count} años. Procesando {aniosPendientes.Length} pendientes.";
            }

            return aniosPendientes;
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
                    // Generar archivo del año
                    var archivoAnio = await ProcesarAnioSegunTipo(
                        config, anio, tipoMigracion, resultado.DirectorioArchivos);

                    if (string.IsNullOrEmpty(archivoAnio))
                        continue;

                    // Dividir y enviar
                    var archivosEnviados = await DividirYEnviarArchivos(
                        archivoAnio, config.NIT, anio, tipoMigracion);

                    totalGenerados += archivosEnviados.Count;
                    totalSubidos += archivosEnviados.Count(a => a.Item2);

                    resultado.AniosProcesados.Add(anio);
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add(new ErrorInfo
                    {
                        Anio = anio,
                        Mensaje = ex.Message,
                        Fecha = DateTime.Now
                    });

                    _fileService.CrearArchivoLog(
                        resultado.DirectorioArchivos,
                        $"ERROR_{config.NIT}_{anio}",
                        $"Error procesando año {anio}: {ex.Message}",
                        true);
                }

                progreso?.Report(Math.Min(20 + ((i + 1) * progresoPorAnio), 80));
                GC.Collect();
            }

            resultado.ArchivosGenerados = totalGenerados.ToString();
            resultado.ArchivosEnviadosAPI = totalSubidos;
        }

        private async Task<List<(string archivo, bool subido)>> DividirYEnviarArchivos(
            string rutaArchivo, string nit, int anio, TipoMigracion tipo)
        {
            var resultados = new List<(string, bool)>();

            // Dividir si es necesario
            var archivos = _fileService.DividirZip(rutaArchivo, _maxZipSizeBytes, _habilitarDivisionZip);

            foreach (var archivo in archivos)
            {
                var nombreArchivo = Path.GetFileName(archivo);
                bool subidoExitosamente = false;

                try
                {
                    // Registrar en log
                    _logService.RegistrarAnioCompletado(nit, anio, tipo, archivos.Count);

                    // Enviar a API
                    var httpAnswer = await _apiService.EnviarArchivoZipAsync(
                        archivo, nit, nombreArchivo, "1");

                    if (httpAnswer.StatusCode == 200)
                    {
                        _logService.ActualizarSubidoS3(nit, anio, tipo, archivos.Count, true, $"{httpAnswer.StatusCode} - {httpAnswer.Content}");
                        subidoExitosamente = true;
                    }
                    _logService.ActualizarSubidoS3(nit, anio, tipo, archivos.Count, false, $"{httpAnswer.StatusCode} - {httpAnswer.Content}");

                }
                catch (Exception ex)
                {
                    _fileService.CrearArchivoLog(
                        Path.GetDirectoryName(archivo),
                        $"ERROR_ENVIO_{Path.GetFileNameWithoutExtension(archivo)}",
                        ex.Message,
                        true);
                    _logService.ActualizarSubidoS3(nit, anio, tipo, archivos.Count, false, $"{ex}");

                }

                resultados.Add((archivo, subidoExitosamente));
            }

            return resultados;
        }

        private async Task<string> ProcesarAnioSegunTipo(
            MigracionConfig config, int anio, TipoMigracion tipo, string directorio)
        {
            switch (tipo)
            {
                case TipoMigracion.Front:
                    return await ProcesarAnio(config, anio, directorio, true, false);
                case TipoMigracion.Back:
                    return await ProcesarAnio(config, anio, directorio, false, true);
                default:
                    return null;
            }
        }

        private async Task<string> ProcesarAnio(
            MigracionConfig config, int anio, string directorio, bool esFront, bool esBack)
        {
            var prefijo = esFront ? "FR" : "BA";
            var db = esFront ? config.DatabaseFront : config.DatabaseBack;

            if (!db.EstaCompleto())
                return null;

            var nombreArchivo = $"{prefijo}_{config.NIT}_{anio}.txt";
            var rutaCompleta = Path.Combine(directorio, nombreArchivo);

            int totalProcesados = 0;
            bool hayDatos = false;

            using (var writer = new StreamWriter(rutaCompleta, false, Encoding.UTF8, bufferSize: 65536))
            {
                await writer.WriteLineAsync("modulo|ldf|parametros|fecha");

                Func<TransaccionData, Task> procesarTransaccion = async (transaccion) =>
                {
                    await writer.WriteLineAsync(FormatearTransaccion(transaccion));
                    totalProcesados++;
                    hayDatos = true;

                    if (totalProcesados % 5000 == 0)
                        await writer.FlushAsync();
                };

                if (esFront)
                    await _databaseService.ConsultarTransaccionesFrontAsync(db, anio, procesarTransaccion);
                else
                    await _databaseService.ConsultarTransaccionesBackAsync(db, anio, procesarTransaccion);

                await writer.FlushAsync();
            }

            if (!hayDatos)
            {
                File.Delete(rutaCompleta);
                return null;
            }

            // Crear ZIP
            var rutaZip = _fileService.CrearArchivoZip(rutaCompleta);
            if (!string.IsNullOrEmpty(rutaZip))
            {
                File.Delete(rutaCompleta);
                rutaCompleta = rutaZip;
            }

            // Log resumen
            _fileService.CrearArchivoLog(
                directorio,
                $"RESUMEN_{prefijo}_{config.NIT}_{anio}",
                $"Año {anio} {prefijo} procesado\n" +
                $"Registros: {totalProcesados:N0}\n" +
                $"Archivo: {Path.GetFileName(rutaCompleta)}\n" +
                $"Tamaño: {new FileInfo(rutaCompleta).Length / 1024:N0} KB"
            );

            return rutaCompleta;
        }

        private static string FormatearTransaccion(TransaccionData t)
        {
            return $"{LimpiarTexto(t.Modulo)}|{LimpiarTexto(t.Mensaje)}|" +
                   $"{LimpiarTexto(t.Parametros)}|{t.FechaHora:yyyy-MM-dd HH:mm:ss}";
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("|", "").Trim();
        }

        private static int[] GenerarAniosDisponibles()
        {
            var anioActual = DateTime.Now.Year;
            return Enumerable.Range(0, 7).Select(i => anioActual - i).ToArray();
        }
    }
}