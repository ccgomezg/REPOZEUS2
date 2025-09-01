using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Services
{
    public interface IApiService
    {
        Task EnviarArchivosAsync(List<string> archivos, string nit, string claveApi);
    }

    public class ApiService : IApiService
    {
        private readonly IFileService _fileService;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _loginEndpoint;
        private readonly string _uploadEndpoint;

        private string _tokenActual;
        private DateTime _tokenExpiracion;

        public ApiService(IFileService fileService)
        {
            _fileService = fileService;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(15) // Más tiempo para archivos grandes
            };

            // URLs configurables - cambiar según tu API real
            _baseUrl = "https://api.miempresa.com";
            _loginEndpoint = "/auth/login";
            _uploadEndpoint = "/migracion/upload";
        }

        public async Task EnviarArchivosAsync(List<string> archivos, string nit, string claveApi)
        {
            if (archivos == null || archivos.Count == 0)
                throw new ArgumentException("No hay archivos para enviar");

            if (string.IsNullOrWhiteSpace(nit) || string.IsNullOrWhiteSpace(claveApi))
                throw new ArgumentException("NIT y clave API son requeridos");

            try
            {
                // Obtener token de autenticación
                await ObtenerTokenAsync(nit, claveApi);

                // Agrupar archivos por año y tipo para crear ZIPs más organizados
                var gruposArchivos = AgruparArchivosPorTipo(archivos);

                foreach (var grupo in gruposArchivos)
                {
                    await EnviarGrupoArchivosAsync(grupo.Value, grupo.Key, nit);
                    await Task.Delay(2000); // Pausa entre grupos
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error enviando archivos a la API: {ex.Message}", ex);
            }
        }

        private async Task ObtenerTokenAsync(string nit, string claveApi)
        {
            // Verificar si el token actual es válido
            if (!string.IsNullOrEmpty(_tokenActual) && DateTime.Now < _tokenExpiracion)
            {
                return; // Token válido, no necesita renovar
            }

            try
            {
                var loginData = new
                {
                    nit = nit,
                    clave = claveApi,
                    aplicacion = "MigracionTool"
                };

                var jsonContent = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}{_loginEndpoint}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parsear respuesta JSON usando Newtonsoft.Json
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    _tokenActual = loginResponse.Token;
                    _tokenExpiracion = DateTime.Now.AddSeconds(loginResponse.ExpiresIn - 60); // 1 min antes de expirar

                    // Configurar header de autorización
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_tokenActual}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error en login - HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obteniendo token de autenticación: {ex.Message}", ex);
            }
        }

        private Dictionary<string, List<string>> AgruparArchivosPorTipo(List<string> archivos)
        {
            var grupos = new Dictionary<string, List<string>>();

            foreach (var archivo in archivos)
            {
                var nombreArchivo = Path.GetFileName(archivo);
                var partes = nombreArchivo.Split('_');

                if (partes.Length >= 3)
                {
                    var tipo = partes[0]; // FR o BA
                    var nit = partes[1];
                    var anio = partes[2].Replace(".txt", "");

                    var clave = $"{tipo}_{anio}";

                    if (!grupos.ContainsKey(clave))
                    {
                        grupos[clave] = new List<string>();
                    }

                    grupos[clave].Add(archivo);
                }
            }

            return grupos;
        }

        private async Task EnviarGrupoArchivosAsync(List<string> archivos, string nombreGrupo, string nit)
        {
            var directorioTrabajo = Path.GetDirectoryName(archivos[0]);
            var nombreZip = $"{nombreGrupo}_{nit}.zip";
            var rutaZip = Path.Combine(directorioTrabajo, nombreZip);

            try
            {
                // Crear archivo ZIP
                CrearArchivoZip(archivos, rutaZip);

                // Enviar ZIP
                await EnviarArchivoZipAsync(rutaZip, nit, nombreGrupo);

                // Limpiar archivo ZIP temporal
                if (File.Exists(rutaZip))
                {
                    File.Delete(rutaZip);
                }
            }
            catch (Exception ex)
            {
                _fileService.CrearArchivoLog(
                    directorioTrabajo,
                    $"ERROR_ZIP_{nombreGrupo}",
                    $"Error procesando grupo {nombreGrupo}: {ex.Message}",
                    true
                );
                throw;
            }
        }

        private void CrearArchivoZip(List<string> archivos, string rutaZip)
        {
            using (var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Create))
            {
                foreach (var archivo in archivos)
                {
                    if (File.Exists(archivo))
                    {
                        var nombreArchivo = Path.GetFileName(archivo);
                        zip.CreateEntryFromFile(archivo, nombreArchivo, CompressionLevel.Optimal);
                    }
                }
            }
        }

        private async Task EnviarArchivoZipAsync(string rutaZip, string nit, string grupo)
        {
            if (!File.Exists(rutaZip))
                throw new FileNotFoundException($"Archivo ZIP no encontrado: {rutaZip}");

            try
            {
                var fileContent = File.ReadAllBytes(rutaZip);
                var fileName = Path.GetFileName(rutaZip);
                var directorio = Path.GetDirectoryName(rutaZip);

                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(new StringContent(nit), "nit");
                    formData.Add(new StringContent(grupo), "tipo_migracion");
                    formData.Add(new StringContent(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "fecha_envio");

                    var fileContentMultipart = new ByteArrayContent(fileContent);
                    fileContentMultipart.Headers.ContentType =
                        System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/zip");
                    formData.Add(fileContentMultipart, "archivo_zip", fileName);

                    var response = await _httpClient.PostAsync($"{_baseUrl}{_uploadEndpoint}", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _fileService.CrearArchivoLog(
                            directorio,
                            $"API_SUCCESS_{grupo}",
                            $"ENVIO EXITOSO\n" +
                            $"Archivo: {fileName}\n" +
                            $"Grupo: {grupo}\n" +
                            $"Tamaño: {fileContent.Length / 1024:N0} KB\n" +
                            $"Respuesta: {responseContent}"
                        );
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        var errorMessage = $"Error HTTP {response.StatusCode}: {errorContent}";

                        _fileService.CrearArchivoLog(
                            directorio,
                            $"API_ERROR_{grupo}",
                            $"ERROR ENVIO API\n" +
                            $"Archivo: {fileName}\n" +
                            $"Grupo: {grupo}\n" +
                            $"Error: {errorMessage}",
                            true
                        );

                        throw new Exception(errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                var directorio = Path.GetDirectoryName(rutaZip);
                var fileName = Path.GetFileName(rutaZip);

                _fileService.CrearArchivoLog(
                    directorio,
                    $"API_EXCEPTION_{grupo}",
                    $"EXCEPCION ENVIO API\n" +
                    $"Archivo: {fileName}\n" +
                    $"Grupo: {grupo}\n" +
                    $"Error: {ex.Message}",
                    true
                );

                throw new Exception($"Error enviando {fileName}: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        // Clase para deserializar respuesta de login
        private class LoginResponse
        {
            public string Token { get; set; }
            public int ExpiresIn { get; set; } // Segundos hasta expiración
            public string RefreshToken { get; set; }
        }
    }
}