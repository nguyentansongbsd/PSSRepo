using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Net;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Net.Http.Headers;
using Azure.Core;
using PssFunctionApp.Entities;
using PssFunctionApp.Reponsitory;
using PssFunctionApp.Reponsitory.Interfaces;

namespace PssFunctionApp
{
    public class GetImages
    {
        private string tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
        private string clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        private string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");  
        private string siteId = Environment.GetEnvironmentVariable("SITE_ID");
        private string driveId = Environment.GetEnvironmentVariable("DRIVE_ID");
        private readonly ILogger<GetImages> _logger;
        private readonly IUnitReponsitory _unitReponsitory;

        public GetImages(ILogger<GetImages> logger, IUnitReponsitory unitReponsitory)
        {
            _logger = logger;
            _unitReponsitory = unitReponsitory;
        }

        [Function("imageUnit")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string id = query["id"];
            _logger.LogInformation($"Received ID: {id}");
            if (string.IsNullOrWhiteSpace(id)) _logger.LogInformation("Chưa có Id SP");

            Unit unit = await _unitReponsitory.getUnitById(id);
            string _id = id.Replace("-", "").ToUpper();
            string folderName = Uri.EscapeDataString(unit.name.Replace(".","-")) + "_" + _id;
            string image = unit.name + ".png";
            string path = $"{folderName}/{image}";

            var token = await GetAccessToken();
            var fileBytes = await DownloadFileFromSharePoint(token, path);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "image/png"); // Thay đổi tùy loại ảnh
            response.Body = new MemoryStream(fileBytes);

            return response;
        }
        private async Task<string> GetAccessToken()
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            string[] scopes = { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }
        private async Task<byte[]> DownloadFileFromSharePoint(string token, string path)
        {
            string url = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:/{path}:/content";
            
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    System.IO.File.WriteAllBytes("images.png", fileBytes);
                    Console.WriteLine("File downloaded successfully.");
                    return fileBytes;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }

            return null;
        }

    }
}
