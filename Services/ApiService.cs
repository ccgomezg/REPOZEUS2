using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Services
{
    public interface IApiService
    {
         Task<(int StatusCode, string Content)> EnviarArchivoZipAsync(string rutaZip, string nit, string ticket, string totalDocumentos, int maxReintentos = 3);
    }

    public class ApiService : IApiService
    {
        private readonly IFileService _fileService;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _loginEndpoint;
        private readonly string _uploadEndpoint;
        private const int MAX_REINTENTOS = 3;

        private string _tokenActual;
        private DateTime _tokenExpiracion;

        public ApiService(IFileService fileService)
        {
            _fileService = fileService;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(15)
            };
            _baseUrl = "https://potential-train-qg4rpv96pvpfxrp7-8000.app.github.dev";
            _loginEndpoint = "/auth/login";
            _uploadEndpoint = "/upload-zip";
        }

        private async Task ObtenerTokenAsync(string nit)
        {

            var formData = new Dictionary<string, string>
            {
                { "accesstoken", "b7fbef73-f359-4408-aba9-53fe78589196" },
                { "nit", nit},
            };

            var content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.PostAsync($"{_baseUrl}{_loginEndpoint}", content);
            var response2 = await _httpClient.GetAsync($"{_baseUrl}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                _tokenActual = loginResponse.Token;
                _tokenExpiracion = DateTime.Now.AddSeconds(loginResponse.ExpiresIn - 60);

                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_tokenActual}");
            }
            else
            {
                //var errorContent = await response.Content.ReadAsStringAsync();
                //throw new Exception($"Error API DE SUBIDA DE ARCHIVOS (SIN ACCESOS) - HTTP {response.StatusCode}: {errorContent}");
            }
        }

        public async Task<(int StatusCode, string Content)> EnviarArchivoZipAsync(
    string rutaZip, string nit, string ticket, string totalDocumentos, int maxReintentos = 3)
        {
            if (!File.Exists(rutaZip))
                throw new FileNotFoundException($"Archivo ZIP no encontrado: {rutaZip}");

            // Login solo si no hay token o está vencido
            if (string.IsNullOrEmpty(_tokenActual) || DateTime.Now >= _tokenExpiracion)
                await ObtenerTokenAsync(nit);

            var fileContent = File.ReadAllBytes(rutaZip);
            var fileName = Path.GetFileName(rutaZip);

            int intento = 0;
            while (intento < maxReintentos)
            {
                try
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        formData.Add(new StringContent(nit), "nit");
                        formData.Add(new StringContent(ticket), "ticket");
                        formData.Add(new StringContent(totalDocumentos), "totalDocumentos");

                        var fileContentMultipart = new ByteArrayContent(fileContent);
                        fileContentMultipart.Headers.ContentType =
                            System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/zip");
                        formData.Add(fileContentMultipart, "archivo", fileName);

                        using (var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{_uploadEndpoint}"))
                        {
                            req.Content = formData;

                            // headers por solicitud (no en DefaultRequestHeaders dentro del bucle)
                            req.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                            req.Headers.Connection.Add("keep-alive");
                            req.Headers.TryAddWithoutValidation("x-sdk-selected-connection", "1");
                            req.Headers.TryAddWithoutValidation("x-sdk-session", "c21703b3-e3b1-4da6-9472-2883000ffd8a");

                            var response = await _httpClient.SendAsync(req);
                            var content = response.ReasonPhrase;
                            var code = (int)response.StatusCode;

                            if (code == 200) return (code, content);

                            if ((code == 401 || code == 403) && intento + 1 < maxReintentos)
                            {
                                await ObtenerTokenAsync(nit);
                                intento++;
                                await Task.Delay(1000);
                                continue;
                            }

                            if ((code == 429 || (code >= 500 && code <= 599)) && intento + 1 < maxReintentos)
                            {
                                intento++;
                                await Task.Delay(2000);
                                continue;
                            }

                            return (code, content);
                        }
                    }
                }
                catch
                {
                    intento++;
                    if (intento >= maxReintentos) throw;
                    await Task.Delay(2000);
                }
            }

            return (0, "Sin respuesta");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private class LoginResponse
        {
            public string Token { get; set; }
            public int ExpiresIn { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}