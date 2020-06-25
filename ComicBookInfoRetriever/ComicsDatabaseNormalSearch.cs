using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Web.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ComicBookInfoRetriever
{
    public class ComicsDatabaseNormalSearch : QueryStrategy
    {
        private string[] requiredParameters;


        public ComicsDatabaseNormalSearch()
        {
            this.requiredParameters = new[] { "issueYear", "issueNumber", "seriesTitle" };
        }


        public override string[] RequiredParameters
        {
            get
            {
                return this.requiredParameters;
            }
        }

        public override async Task<WeightedResult> Execute((string parameterName, string parameterValue)[] parameters, HttpClient httpClient)
        {
            WeightedResult weightedResult = new WeightedResult();

            string issueYear = parameters.First(x => x.parameterName.Equals("issueYear", StringComparison.InvariantCultureIgnoreCase)).parameterValue;
            string issueNumber = parameters.First(x => x.parameterName.Equals("issueNumber", StringComparison.InvariantCultureIgnoreCase)).parameterValue;
            string seriesTitle = parameters.First(x => x.parameterName.Equals("seriesTitle", StringComparison.InvariantCultureIgnoreCase)).parameterValue;

            using (var standardSearchRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://www.comics.org/searchNew/?q={WebUtility.UrlEncode(seriesTitle)}+{issueNumber}+{issueYear}+&search_object=issue")))
            {
                standardSearchRequest.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                standardSearchRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                standardSearchRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                standardSearchRequest.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                using (var standardSearchResponse = await httpClient.SendAsync(standardSearchRequest).ConfigureAwait(false))
                {
                    standardSearchResponse.EnsureSuccessStatusCode();
                    using (var standardSearchResponseStream = await standardSearchResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var standardSearchDecompressedStream = new GZipStream(standardSearchResponseStream, CompressionMode.Decompress))
                    using (var standardSearchStreamReader = new StreamReader(standardSearchDecompressedStream))
                    {
                        var standardSearchResult = await standardSearchStreamReader.ReadToEndAsync().ConfigureAwait(false);
                        HtmlParser parser = new HtmlParser();
                        IHtmlDocument standardSearchDocument = parser.ParseDocument(standardSearchResult);
                        var resultTable = standardSearchDocument.GetElementsByClassName("listing left");                       

                        if (resultTable.Count() > 0)
                        {
                            var issue = resultTable.First().GetElementsByTagName("TR").Where(x => x.InnerHtml.Contains(seriesTitle, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(issueYear, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(issueYear, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            
                            if(issue != null)
                            {
                                var anchor = issue.GetElementsByTagName("A").FirstOrDefault();
                                var urlSection = anchor?.Attributes.FirstOrDefault();

                                UriBuilder builder = new UriBuilder("https", "comics.org", -1, urlSection.Value);

                                using (var issuePageRequest = new HttpRequestMessage(HttpMethod.Get, builder.Uri))
                                {
                                    issuePageRequest.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                                    issuePageRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                                    issuePageRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                                    issuePageRequest.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                                    using (var issuePageResponse = await httpClient.SendAsync(issuePageRequest).ConfigureAwait(false))
                                    {
                                        issuePageResponse.EnsureSuccessStatusCode();
                                        using (var issuePageResponseStream = await issuePageResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                        using (var issuePageResponseDecompressedStream = new GZipStream(issuePageResponseStream, CompressionMode.Decompress))
                                        using (var issuePageResponseStreamReader = new StreamReader(issuePageResponseDecompressedStream))
                                        {
                                            var issuePageResult = await issuePageResponseStreamReader.ReadToEndAsync().ConfigureAwait(false);
                                            HtmlParser issuePageparser = new HtmlParser();
                                            IHtmlDocument issuePageDocument = parser.ParseDocument(issuePageResult);
                                            var coverImage = issuePageDocument.Images.Where(x => x.ClassName == "cover_img").FirstOrDefault();

                                            if(coverImage != null)
                                            {
                                                weightedResult.ImageSource = coverImage.Source;
                                                weightedResult.Success = true;
                                            }

                                        }
                                    }
                                }
                            }
                            else
                            {
                                weightedResult.Success = false;
                            }                            
                        }
                        else
                        {
                            weightedResult.Success = false;
                        }
                        
                    }
                    return weightedResult;
                    
                }
            }

        }

    }
    
} 
