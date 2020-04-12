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

namespace ComicBookInfoRetriever
{
    public static class RetrieveCover
    {
        private static HttpClient httpClient = new HttpClient();
        [FunctionName("RetrieveCover")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string issueNumber = "528";
                string seriesTitle = "Adventures of Superman";
                string year = "1995";

                log.LogInformation("C# HTTP trigger function processed a request.");

                string name = req.Query["name"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                name = name ?? data?.name;

                string responseMessage = string.IsNullOrEmpty(name)
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, {name}. This HTTP triggered function executed successfully.";
                string url = "https://www.comics.org/search/advanced/process/?target=issue_cover&method=icontains&logic=False&keywords=&title=&feature=&job_number=&pages=&pages_uncertain=&script=&pencils=&inks=&colors=&letters=&story_editing=&first_line=&characters=&synopsis=&reprint_notes=&story_reprinted=&notes=&issues=528&volume=&issue_title=&variant_name=&is_variant=&issue_date=&indicia_frequency=&price=&issue_pages=&issue_pages_uncertain=&issue_editing=&isbn=&barcode=&rating=&issue_notes=&issue_reprinted=&is_indexed=&in_selected_collection=on&order1=series&order2=date&order3=&start_date=&end_date=&updated_since=&pub_name=&pub_notes=&brand_group=&brand_emblem=&brand_notes=&indicia_publisher=&is_surrogate=&ind_pub_notes=&series=Adventures+of+Superman&series_year_began=&series_notes=&tracking_notes=&issue_count=&is_comics=&color=&dimensions=&paper_stock=&binding=&publishing_format=";
                using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                    request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
                    using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        using (var streamReader = new StreamReader(decompressedStream))
                        {
                            var stringResult = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                            HtmlParser parser = new HtmlParser();
                            IHtmlDocument document = parser.ParseDocument(stringResult);
                            var issue = document.All.FirstOrDefault(x => x.ClassName == "covers" && x.InnerHtml.Contains(issueNumber) && x.InnerHtml.Contains(seriesTitle) && x.InnerHtml.Contains(year));

                            var src = issue.Children.ElementAt(0).Children.ElementAt(0).Children.ElementAt(0).Attributes.ElementAt(0).Value;
                         
                        }
                    }
                }
                return new OkObjectResult(responseMessage);

            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }
           
        }
    }
}
