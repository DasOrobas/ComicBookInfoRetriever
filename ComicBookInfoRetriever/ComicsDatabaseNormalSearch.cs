using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComicBookInfoRetriever
{
    public class ComicsDatabaseNormalSearch : QueryStrategy
    {
        private string[] requiredParameters;


        public ComicsDatabaseNormalSearch()
        {
            this.requiredParameters = new[] { "issueYear", "issueYear", "seriesTitle" };
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
                        var issues = standardSearchDocument.All.Where(x => x.ClassName == "listing_even" || x.ClassName == "listing_odd");
                    }
                    //        standardSearchResponse.EnsureSuccessStatusCode();
                    //        using (var standardSearchResponseStream = await standardSearchResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    //        using (var standardSearchDecompressedStream = new GZipStream(standardSearchResponseStream, CompressionMode.Decompress))
                    //        using (var standardSearchStreamReader = new StreamReader(standardSearchDecompressedStream))
                    //        {
                    //            var standardSearchResult = await standardSearchStreamReader.ReadToEndAsync().ConfigureAwait(false);
                    //            HtmlParser parser = new HtmlParser();
                    //            IHtmlDocument standardSearchDocument = parser.ParseDocument(standardSearchResult);
                    //            var issues = standardSearchDocument.All.Where(x => x.ClassName == "listing_even" || x.ClassName == "listing_odd");

                    //            //var issue = issues.FirstOrDefault(x => x.InnerHtml.Contains(issueNumber, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(seriesTitle, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(year));


                    //          //  if (issue != null)
                    //            //{

                    //                //using (var issueWebPageResponse = await httpClient.SendAsync(request).ConfigureAwait(false))
                    //                {

                    //                }

                }
                //            else
                //            {

                //                //return new NotFoundObjectResult($"{seriesTitle} {issueNumber} {issueNumber}");
            }

                return weightedResult;
                    }
                }
            }

    //    }

    //}
    
//}
