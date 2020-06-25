using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using System.IO.Compression;
using AngleSharp.Dom;
using System.Collections.Generic;

namespace ComicBookInfoRetriever
{
    public static class RetrieveCover
    {
        private static HttpClient httpClient = new HttpClient();
        [FunctionName("RetrieveCover")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                
                string issueNumber = req.Query["issueNumber"];
                string seriesTitle = req.Query["seriesName"];
                string year = req.Query["year"];

                issueNumber = issueNumber?.Trim();
                seriesTitle = seriesTitle?.Trim();
                year = year?.Trim();

                List<(string, string)> parameters = new List<(string, string)>
                {
                    ("issueNumber", issueNumber),
                    ("seriesTitle", seriesTitle),
                    ("issueYear", year)
                };

                string targetFileName = $"{seriesTitle}_{issueNumber}_{year}.jpg";
                string targetFilePath = Path.Combine(Path.GetTempPath(), targetFileName);
                if (File.Exists(targetFilePath))
                {
                    return new PhysicalFileResult(targetFilePath, "image/jpeg");
                }
                else
                {


                    ComicsDatabaseNormalSearch normalSearch = new ComicsDatabaseNormalSearch();
                    var result = await normalSearch.Execute(parameters.ToArray(), httpClient);

                    if (!result.Success)
                    {
                        ComicsDatabaseAdvancedQuery advancedQuery = new ComicsDatabaseAdvancedQuery();
                        result = await advancedQuery.Execute(parameters.ToArray(), httpClient);
                    }

                    if (result.Success)
                    {
                        if (!File.Exists(targetFilePath))
                        {
                            var downloadedFilePath = await DownloadFile(result.ImageSource, targetFilePath);                            
                        }

                        return new PhysicalFileResult(targetFilePath, "image/jpeg");
                    }
                    else
                    {
                        return new NotFoundObjectResult($"{seriesTitle} {issueNumber} {issueNumber}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }
           
        }

        public static async Task<string> DownloadFile(string url, string targetFilePath)
        {
            // check if png is possible in db

            var fileInfo = new FileInfo(targetFilePath);

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using (var ms = await response.Content.ReadAsStreamAsync())
            {
                using (var fs = File.Create(fileInfo.FullName))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }

            }

            return fileInfo.FullName;
        }
    }
}
