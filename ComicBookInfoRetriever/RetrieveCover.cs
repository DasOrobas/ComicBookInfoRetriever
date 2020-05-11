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

                log.LogInformation("C# HTTP trigger function processed a request.");    

                  string url = $"https://www.comics.org/search/advanced/process/?target=issue_cover&method=icontains&logic=False&keywords=&title=&feature=&job_number=&pages=&pages_uncertain=&script=&pencils=&inks=&colors=&letters=&story_editing=&first_line=&characters=&synopsis=&reprint_notes=&story_reprinted=&notes=&issues={issueNumber}&volume=&issue_title=&variant_name=&is_variant=&issue_date=&indicia_frequency=&price=&issue_pages=&issue_pages_uncertain=&issue_editing=&isbn=&barcode=&rating=&issue_notes=&issue_reprinted=&is_indexed=&in_selected_collection=on&order1=series&order2=date&order3=&start_date=&end_date=&updated_since=&pub_name=&pub_notes=&brand_group=&brand_emblem=&brand_notes=&indicia_publisher=&is_surrogate=&ind_pub_notes=&series={seriesTitle.Replace(" ", "+")}&series_year_began=&series_notes=&tracking_notes=&issue_count=&is_comics=&color=&dimensions=&paper_stock=&binding=&publishing_format=";
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
                            var issues = document.All.Where(x => x.ClassName == "covers" && x.InnerHtml.Contains(issueNumber, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(seriesTitle, StringComparison.InvariantCultureIgnoreCase));

                            IElement issue;
                            if (year == null)
                            {
                                issue = issues.FirstOrDefault();
                            }
                            else
                            {
                                issue = issues.FirstOrDefault(x => x.InnerHtml.Contains(year, StringComparison.InvariantCultureIgnoreCase));
                            }

                            if (issue == null)
                            {

                                using (var standardSearchRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://www.comics.org/searchNew/?q={WebUtility.UrlEncode(seriesTitle)}+{issueNumber}+{year}+&search_object=issue")))
                                {
                                    standardSearchRequest.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                                    standardSearchRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                                    standardSearchRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                                    standardSearchRequest.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                                    using (var standardSearchResponse = await httpClient.SendAsync(standardSearchRequest).ConfigureAwait(false))
                                    {
                                        response.EnsureSuccessStatusCode();
                                        using (var standardSearchResponseStream = await standardSearchResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                        using (var standardSearchDecompressedStream = new GZipStream(standardSearchResponseStream, CompressionMode.Decompress))
                                        using (var standardSearchStreamReader = new StreamReader(standardSearchDecompressedStream))
                                        {
                                            var standardSearchResult = await standardSearchStreamReader.ReadToEndAsync().ConfigureAwait(false);
                                            IHtmlDocument standardSearchDocument = parser.ParseDocument(standardSearchResult);
                                            issues = standardSearchDocument.All.Where(x => x.ClassName == "listing_even" || x.ClassName == "listing_odd");

                                            if (year == null)
                                            {
                                                issue = issues.FirstOrDefault(x => x.InnerHtml.Contains(issueNumber, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(seriesTitle, StringComparison.InvariantCultureIgnoreCase));
                                            }         
                                            else
                                            {
                                                issue = issues.FirstOrDefault(x => x.InnerHtml.Contains(issueNumber, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(seriesTitle, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(year));
                                            }

                                            if (issue != null)
                                            {

                                                using (var issueWebPageResponse = await httpClient.SendAsync(request).ConfigureAwait(false))
                                                {

                                                }

                                            }
                                            else
                                            {

                                                return new NotFoundObjectResult($"{seriesTitle} {issueNumber} {issueNumber}");
                                            }
                                        }
                                    }
                                }

                            }

                            var src = issue.Children.ElementAt(0).Children.ElementAt(0).Children.ElementAt(0).Attributes.ElementAt(0).Value;



                            string targetFileName = $"{seriesTitle}_{issueNumber}_{year}.jpg";
                            string targetFilePath = Path.Combine(Path.GetTempPath(), targetFileName);
                            if (!File.Exists(targetFilePath))
                            {
                                var downloadedFilePath = await DownloadFile(src, targetFilePath);
                            }               
                                       
                            return new PhysicalFileResult(targetFilePath, "image/jpeg");
                        }
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
