using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IFileService
    {
        string GenerarArchivoTransacciones(string rutaArchivo, int anio, List<TransaccionData> transaccionesFront, List<TransaccionData> transaccionesBack);
        string CrearDirectorioMigracion(string rutaBase, string nit);
        void CrearArchivoLog(string directorio, string nombreArchivo, string contenido, bool esError = false);
        string CrearArchivoZip(string rutaArchivo);

        List<string> DividirZip(
            string rutaZip,
            long maxSizeBytes,
            bool habilitarDivision = true);
    }

    public class FileService : IFileService
    {
        public string GenerarArchivoTransacciones(string rutaArchivo, int anio,
            List<TransaccionData> transaccionesFront, List<TransaccionData> transaccionesBack)
        {
            try
            {
                var todas = new List<TransaccionData>();

                if (transaccionesFront != null)
                    todas.AddRange(transaccionesFront);

                if (transaccionesBack != null)
                    todas.AddRange(transaccionesBack);

                var ordenadas = todas.OrderBy(t => t.FechaHora);

                using (var writer = new StreamWriter(rutaArchivo, false, Encoding.UTF8))
                {
                    //writer.WriteLine("modulo|ldf|parametros|fecha");

                    foreach (var transaccion in ordenadas)
                    {
                        writer.WriteLine(
                            $"{LimpiarTexto(transaccion.Modulo)}|" +
                            $"{LimpiarTexto(transaccion.Mensaje)}|" +
                            $"{LimpiarTexto(transaccion.Parametros)}|" +
                            $"{transaccion.FechaHora:yyyy-MM-dd HH:mm:ss}"
                        );
                    }
                }

                return rutaArchivo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generando archivo {rutaArchivo}: {ex.Message}", ex);
            }
        }

        public string CrearArchivoZip(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
                return null;

            try
            {
                var directorioArchivo = Path.GetDirectoryName(rutaArchivo);
                var nombreSinExtension = Path.GetFileNameWithoutExtension(rutaArchivo);
                var rutaZip = Path.Combine(directorioArchivo, $"{nombreSinExtension}.zip");

                //Eliminar ZIP existente si existe
                if (File.Exists(rutaZip))
                    File.Delete(rutaZip);

                using (var archivo = ZipFile.Open(rutaZip, ZipArchiveMode.Create))
                {
                    archivo.CreateEntryFromFile(rutaArchivo, Path.GetFileName(rutaArchivo), CompressionLevel.Optimal);
                }

                return rutaZip;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creando archivo ZIP: {ex.Message}", ex);
            }
        }

        public string CrearDirectorioMigracion(string rutaBase, string nit)
        {
            var nombreDirectorio = $"Migracion_{nit}_{DateTime.Now:yyyyMMdd_HHmmss}";
            var rutaCompleta = Path.Combine(rutaBase, "MIGRACIONES", nombreDirectorio);

            Directory.CreateDirectory(rutaCompleta);
            return rutaCompleta;
        }

        public void CrearArchivoLog(string directorio, string nombreArchivo, string contenido, bool esError = false)
        {
            var prefijo = esError ? "ERROR" : "LOG";
            var nombreCompleto = $"{prefijo}_{nombreArchivo}.txt";
            var rutaCompleta = Path.Combine(directorio, nombreCompleto);

            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var contenidoCompleto = $"{timestamp}\n{contenido}";

            File.WriteAllText(rutaCompleta, contenidoCompleto, Encoding.UTF8);
        }

        // MÉTODO OPTIMIZADO: Divide en ZIPs con UN SOLO archivo TXT cada uno
        public List<string> DividirZip(
            string rutaZip,
            long maxSizeBytes,
            bool habilitarDivision = true)
        {
            // Caso simple: no dividir
            if (!habilitarDivision || new FileInfo(rutaZip).Length <= maxSizeBytes)
            {
                return new List<string> { rutaZip };
            }

            var archivosResultado = new List<string>();
            var directorioTemporal = Path.Combine(
                Path.GetDirectoryName(rutaZip),
                $"temp_{Guid.NewGuid()}");

            try
            {
                Directory.CreateDirectory(directorioTemporal);

                // Extraer el ZIP original
                ZipFile.ExtractToDirectory(rutaZip, directorioTemporal);

                // Buscar el archivo TXT principal
                var archivosTxt = Directory.GetFiles(directorioTemporal, "*.txt", SearchOption.AllDirectories);

                if (archivosTxt.Length == 0)
                {
                    // No hay archivos TXT, devolver el original
                    return new List<string> { rutaZip };
                }

                // Procesar el archivo TXT principal (asumiendo que hay uno solo o procesamos el primero)
                var archivoTxtPrincipal = archivosTxt[0];
                var nombreArchivoOriginal = Path.GetFileNameWithoutExtension(archivoTxtPrincipal);

                // Dividir directamente en ZIPs
                archivosResultado = CrearZipsConDivisionDirecta(
                    archivoTxtPrincipal,
                    rutaZip,
                    maxSizeBytes);

                // Solo eliminar el original si se crearon nuevos archivos
                if (archivosResultado.Count > 0)
                {
                    File.Delete(rutaZip);
                }
            }
            catch (Exception ex)
            {
                
                if (archivosResultado.Count == 0 && File.Exists(rutaZip))
                {
                    archivosResultado.Add(rutaZip);
                }
                throw new InvalidOperationException($"Error al dividir el archivo: {ex.Message}", ex);
            }
            finally
            {
                
                if (Directory.Exists(directorioTemporal))
                {
                    try { Directory.Delete(directorioTemporal, true); } catch { }
                }
            }

            return archivosResultado;
        }

        // Crear ZIPs dividiendo el contenido directamente
        private List<string> CrearZipsConDivisionDirecta(
    string archivoTxtOriginal,
    string rutaZipOriginal,
    long maxSizeBytes)
        {
            var archivosResultado = new List<string>();
            var directorio = Path.GetDirectoryName(rutaZipOriginal);
            var nombreBase = Path.GetFileNameWithoutExtension(rutaZipOriginal);
            var nombreArchivoTxt = Path.GetFileName(archivoTxtOriginal);
            // Calcular cuántas líneas aproximadamente caben en cada ZIP
            // Considerando factor de compresión de ~15% para TXT
            var maxBytesDescomprimido = (long)(maxSizeBytes / 0.15);
            int parteNumero = 1;
            using (var lector = new StreamReader(archivoTxtOriginal, Encoding.UTF8))
            {
                var lineasBuffer = new List<string>();
                long bytesAcumulados = 0;
                string linea;
                while ((linea = lector.ReadLine()) != null)
                {
                    var bytesLinea = Encoding.UTF8.GetByteCount(linea + Environment.NewLine);
                    // Si agregar esta línea excede el límite, crear el ZIP con lo acumulado
                    if (bytesAcumulados + bytesLinea > maxBytesDescomprimido && lineasBuffer.Count > 0)
                    {
                        // Crear ZIP con el buffer actual
                        var rutaZipParte = CrearZipConContenido(
                            directorio,
                            nombreBase,
                            nombreArchivoTxt,
                            parteNumero++,
                            lineasBuffer,
                            maxSizeBytes);
                        if (!string.IsNullOrEmpty(rutaZipParte))
                        {
                            archivosResultado.Add(rutaZipParte);
                        }
                        // Reiniciar buffer VACÍO (sin encabezado)
                        lineasBuffer.Clear();
                        bytesAcumulados = 0;
                    }
                    // Agregar línea al buffer
                    lineasBuffer.Add(linea);
                    bytesAcumulados += bytesLinea;
                }
                // Crear ZIP con las líneas restantes
                if (lineasBuffer.Count > 0)
                {
                    var rutaZipParte = CrearZipConContenido(
                        directorio,
                        nombreBase,
                        nombreArchivoTxt,
                        parteNumero,
                        lineasBuffer,
                        maxSizeBytes);
                    if (!string.IsNullOrEmpty(rutaZipParte))
                    {
                        archivosResultado.Add(rutaZipParte);
                    }
                }
            }
            return archivosResultado;
        }

        // Crear un ZIP con el contenido especificado
        private string CrearZipConContenido(
            string directorio,
            string nombreBase,
            string nombreArchivoTxt,
            int parteNumero,
            List<string> lineas,
            long maxSizeBytes)
        {
            var nombreZipParte = $"{nombreBase}_{parteNumero:D3}.zip";
            var rutaZipParte = Path.Combine(directorio, nombreZipParte);
            var rutaTxtTemporal = Path.Combine(directorio, $"temp_{Guid.NewGuid()}.txt");

            try
            {
                // Escribir el archivo TXT temporal
                File.WriteAllLines(rutaTxtTemporal, lineas, Encoding.UTF8);

                // Crear el ZIP con el archivo temporal
                using (var zip = ZipFile.Open(rutaZipParte, ZipArchiveMode.Create))
                {
                    // Mantener el nombre original del archivo dentro del ZIP
                    zip.CreateEntryFromFile(rutaTxtTemporal, nombreArchivoTxt, CompressionLevel.Optimal);
                }

                // Verificar que el ZIP no exceda el límite
                var tamañoZip = new FileInfo(rutaZipParte).Length;

                if (tamañoZip > maxSizeBytes)
                {
                    // Si excede, necesitamos dividir más fino
                    File.Delete(rutaZipParte);

                    // Reducir el número de líneas y reintentar
                    var lineasReducidas = lineas.Take(lineas.Count * 3 / 4).ToList();
                    if (lineasReducidas.Count > 1)
                    {
                        return CrearZipConContenido(
                            directorio,
                            nombreBase,
                            nombreArchivoTxt,
                            parteNumero,
                            lineasReducidas,
                            maxSizeBytes);
                    }

                    return null;
                }

                return rutaZipParte;
            }
            finally
            {
                // Limpiar archivo temporal
                if (File.Exists(rutaTxtTemporal))
                {
                    try { File.Delete(rutaTxtTemporal); } catch { }
                }
            }
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return texto.Replace("\r", "")
                       .Replace("\n", "")
                       .Replace("\t", "")
                       .Trim();
        }
    }
}