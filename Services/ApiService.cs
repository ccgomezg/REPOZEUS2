using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1.Services
{
    public interface IApiService
    {
        Task<(int StatusCode, string Content)> EnviarArchivoZipAsync(string rutaZip, string nit, string ticket, string totalDocumentos, int maxReintentos = 3);
        Task<(int StatusCode, string Content)> IniciarMigracionAsync(string nit, string ticket, string totalDocumentos, string fechaDesde, string fechaHasta, int maxReintentos = 3);
        void CambiarAmbiente(int ambiente);
    }

    public class ApiService : IApiService
    {
        private readonly IFileService _fileService;
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private readonly string _baseUrlQa;
        private readonly string _baseUrlReal;
        private readonly string _loginEndpoint;
        private readonly string _uploadEndpoint;
        private readonly string _create_ticket;
        private const int MAX_REINTENTOS = 3;

        private string _tokenActual;
        private DateTime _tokenExpiracion;

        public ApiService(IFileService fileService, int ambiente =0)
        {
            _fileService = fileService;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(15)
            };

            _baseUrlQa = "https://pdfsdeqa.siesacloud.com";
            _baseUrlReal = "https://pdfsdepro.siesacloud.com";

            _baseUrl = ambiente == 1 ? _baseUrlQa : _baseUrlReal;

            _loginEndpoint = "/api/login";
            _uploadEndpoint = "/api/BLMigrate/FileUploadS3";
            _create_ticket = "/api/BLMigrate/StartMigrate";
            //_uploadEndpoint = "/api/BLMig";
        }

        private async Task ObtenerTokenAsync(string nit)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ZeusApp/1.0");
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("x-sdk-selected-connection", "1");

            var formData = new Dictionary<string, string>
            {
                { "accesstoken", "1b148880-fdf0-4409-b060-b80603657f8e" } 
            };

            var content = new FormUrlEncodedContent(formData);
            var fullUrl = $"{_baseUrl}{_loginEndpoint}";
            var bodyContent = await content.ReadAsStringAsync();
            
            var response = await _httpClient.PostAsync(fullUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                _tokenActual = loginResponse.Data;
                _tokenExpiracion = DateTime.Now.AddSeconds(300);

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

            if (string.IsNullOrEmpty(_tokenActual) || DateTime.Now >= _tokenExpiracion)
                await ObtenerTokenAsync(nit);

            var fileContent = File.ReadAllBytes(rutaZip);
            var fileName = Path.GetFileName(rutaZip);
            int intento = 0;
            string ticketSinExtension  =  ticket.Split('.')[0];

            while (intento < maxReintentos)
            {
                try
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        // Datos del formulario
                        formData.Add(new StringContent(nit), "nit");
                        formData.Add(new StringContent(ticketSinExtension), "ticket");
                        formData.Add(new StringContent(totalDocumentos), "totalDocumentos");

                        // Archivo
                        var fileContentMultipart = new ByteArrayContent(fileContent);
                        fileContentMultipart.Headers.ContentType =
                            MediaTypeHeaderValue.Parse("application/zip");
                        formData.Add(fileContentMultipart, "archivo", fileName);

                        using (var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{_uploadEndpoint}"))
                        {
                            req.Content = formData;

                            req.Headers.Add("Accept", "*/*");
                            req.Headers.Add("Cache-Control", "no-cache");
                            req.Headers.Add("x-sdk-selected-connection", "1");
                            req.Headers.Add("x-sdk-session", _tokenActual);


                            var response = await _httpClient.SendAsync(req);
                            var responseContent = await response.Content.ReadAsStringAsync();
                            var content = JObject.Parse(responseContent);
                            var contentString = content.ToString();
                            var code = int.Parse(content["data"]["Code"].ToString());

                            //var content = await response.Content.ReadAsStringAsync();
                            //var code = (int)response.StatusCode;

                            Console.WriteLine($"Status: {code}, Response: {content}");

                            if (code == 200) return (code, contentString);

                            if ((code == 401 || code == 403) && intento + 1 < maxReintentos)
                            {
                                Console.WriteLine("Token expirado, renovando...");
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

                            return (code, contentString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en intento {intento + 1}: {ex.Message}");
                    intento++;
                    if (intento >= maxReintentos) throw;
                    await Task.Delay(2000);
                }
            }

            return (0, "Sin respuesta");
        }


        public async Task<(int StatusCode, string Content)> IniciarMigracionAsync(
            string nit,
            string ticket,
            string totalDocumentos,
            string fechaDesde,
            string fechaHasta,
            int maxReintentos = 3
        )
        {
             if (string.IsNullOrEmpty(_tokenActual) || DateTime.Now >= _tokenExpiracion)
                await ObtenerTokenAsync(nit);

            int intento = 0;

            while (intento < maxReintentos)
            {
                try
                {
                    var formData = new Dictionary<string, string>
                    {
                        { "nit", nit },
                        { "ticket", ticket },
                        { "totalDocumentos", totalDocumentos },
                        { "fechaDesde", fechaDesde },
                        { "fechaHasta", fechaHasta }
                    };

                    var content = new FormUrlEncodedContent(formData);

                    using (var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/BLMigrate/StartMigrate"))
                    {
                        req.Content = content;

                        req.Headers.Add("Accept", "*/*");
                        req.Headers.Add("Cache-Control", "no-cache");
                        req.Headers.Add("x-sdk-selected-connection", "1");
                        req.Headers.Add("x-sdk-session", _tokenActual);


                        var response = await _httpClient.SendAsync(req);
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var contentJson = JObject.Parse(responseContent);
                        var code = int.Parse(contentJson["data"]["Code"].ToString());
                        

                        Console.WriteLine($"Status: {code}");
                        Console.WriteLine($"Response: {responseContent}");

                        if (code == 200) return (code, responseContent);

                        if ((code == 401 || code == 403) && intento + 1 < maxReintentos)
                        {
                            Console.WriteLine("Token expirado, renovando...");
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

                        return (code, responseContent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en intento {intento + 1}: {ex.Message}");
                    intento++;
                    if (intento >= maxReintentos) throw;
                    await Task.Delay(2000);
                }
            }

            return (0, "Sin respuesta después de todos los reintentos");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public void CambiarAmbiente(int ambiente)
        {
            if (ambiente == 1)
                this._baseUrl = "https://pdfsdeqa.siesacloud.com";
            else
                this._baseUrl = "https://pdfsdepro.siesacloud.com";
        }

        private class LoginResponse
        {
            public string Data { get; set; }
            public string ExpiresIn { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}