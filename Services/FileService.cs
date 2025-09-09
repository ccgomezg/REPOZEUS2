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
                    writer.WriteLine("modulo|ldf|parametros|fecha");

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

        public List<string> DividirZip(
            string rutaZip,
            long maxSizeBytes,
            bool habilitarDivision = true)
        {
            var archivosResultado = new List<string>();
            var infoZip = new FileInfo(rutaZip);

            // Si no está habilitada la división o el archivo es pequeño, devolver tal cual
            if (!habilitarDivision || infoZip.Length <= maxSizeBytes)
            {
                archivosResultado.Add(rutaZip);
                return archivosResultado;
            }

            // Dividir el ZIP
            var directorio = Path.GetDirectoryName(rutaZip);
            var nombreBase = Path.GetFileNameWithoutExtension(rutaZip);
            var directorioTemporal = Path.Combine(directorio, $"temp_{Guid.NewGuid()}");

            try
            {
                Directory.CreateDirectory(directorioTemporal);
                ZipFile.ExtractToDirectory(rutaZip, directorioTemporal);

                var archivosExtraidos = Directory.GetFiles(directorioTemporal, "*", SearchOption.AllDirectories)
                    .Select(f => new { Ruta = f, Tamaño = new FileInfo(f).Length })
                    .OrderBy(f => f.Tamaño)
                    .ToList();

                var gruposArchivos = new List<List<string>>();
                var grupoActual = new List<string>();
                long tamañoActual = 0;

                foreach (var archivo in archivosExtraidos)
                {
                    // Si agregar este archivo excede el límite, crear nuevo grupo
                    if (tamañoActual > 0 && tamañoActual + archivo.Tamaño > maxSizeBytes)
                    {
                        gruposArchivos.Add(new List<string>(grupoActual));
                        grupoActual.Clear();
                        tamañoActual = 0;
                    }

                    grupoActual.Add(archivo.Ruta);
                    tamañoActual += archivo.Tamaño;
                }

                // Agregar último grupo
                if (grupoActual.Count > 0)
                {
                    gruposArchivos.Add(grupoActual);
                }

                // Crear un ZIP para cada grupo
                for (int i = 0; i < gruposArchivos.Count; i++)
                {
                    var nombreZipParte = i == 0
                        ? $"{nombreBase}.zip"
                        : $"{nombreBase}_{i + 1}.zip";

                    var rutaZipParte = Path.Combine(directorio, nombreZipParte);

                    using (var zip = ZipFile.Open(rutaZipParte, ZipArchiveMode.Create))
                    {
                        foreach (var archivo in gruposArchivos[i])
                        {
                            var nombreArchivo = Path.GetFileName(archivo);
                            zip.CreateEntryFromFile(archivo, nombreArchivo, CompressionLevel.Optimal);
                        }
                    }

                    archivosResultado.Add(rutaZipParte);
                }

                // Eliminar ZIP original si se dividió
                if (archivosResultado.Count > 1)
                {
                    File.Delete(rutaZip);
                }
            }
            finally
            {
                // Limpiar directorio temporal
                if (Directory.Exists(directorioTemporal))
                {
                    Directory.Delete(directorioTemporal, true);
                }
            }

            return archivosResultado;
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