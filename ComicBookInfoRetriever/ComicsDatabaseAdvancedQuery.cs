using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComicBookInfoRetriever
{
    public class ComicsDatabaseAdvancedQuery : QueryStrategy
    {
        private string[] requiredParameters;


        public ComicsDatabaseAdvancedQuery()
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

            string url = $"https://www.comics.org/search/advanced/process/?target=issue&method=icontains&logic=False&keywords=&title=&feature=&job_number=&pages=&pages_uncertain=&script=&pencils=&inks=&colors=&letters=&story_editing=&first_line=&characters=&synopsis=&reprint_notes=&story_reprinted=&notes=&issues={issueNumber}&volume=&issue_title=&variant_name=&is_variant=&issue_date=&indicia_frequency=&price=&issue_pages=&issue_pages_uncertain=&issue_editing=&isbn=&barcode=&rating=&issue_notes=&issue_reprinted=&is_indexed=&in_selected_collection=on&order1=series&order2=date&order3=&start_date=&end_date=&updated_since=&pub_name=&pub_notes=&brand_group=&brand_emblem=&brand_notes=&indicia_publisher=&is_surrogate=&ind_pub_notes=&series={seriesTitle.Replace(" ", "+")}&series_year_began=&series_notes=&tracking_notes=&issue_count=&is_comics=&color=&dimensions=&paper_stock=&binding=&publishing_format=";
            using (var advancedQueryRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
            {
                advancedQueryRequest.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                advancedQueryRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                advancedQueryRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                advancedQueryRequest.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                using (var standardSearchResponse = await httpClient.SendAsync(advancedQueryRequest).ConfigureAwait(false))
                {
                    standardSearchResponse.EnsureSuccessStatusCode();
                    using (var standardSearchResponseStream = await standardSearchResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var standardSearchDecompressedStream = new GZipStream(standardSearchResponseStream, CompressionMode.Decompress))
                    using (var standardSearchStreamReader = new StreamReader(standardSearchDecompressedStream))
                    {
                        var standardSearchResult = await standardSearchStreamReader.ReadToEndAsync().ConfigureAwait(false);
                        HtmlParser parser = new HtmlParser();
                        IHtmlDocument standardSearchDocument = parser.ParseDocument(standardSearchResult);
                        var resultTableEven = standardSearchDocument.GetElementsByClassName("listing_even");
                        var resultTableOdd = standardSearchDocument.GetElementsByClassName("listing_odd");

                        var allResultsTable = resultTableEven.Union(resultTableOdd);

                        if (allResultsTable.Count() > 0)
                        {
                            var issue = allResultsTable.Where(x => x.InnerHtml.Contains(seriesTitle, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(issueYear, StringComparison.InvariantCultureIgnoreCase) && x.InnerHtml.Contains(issueYear, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                            if (issue != null)
                            {
                                var anchor = issue.GetElementsByTagName("A").SingleOrDefault(x => x.OuterHtml.Contains(@"/issue/", StringComparison.InvariantCultureIgnoreCase));
                                var urlSection = anchor?.Attributes.FirstOrDefault();

                                UriBuilder builder = new UriBuilder("https", "comics.org", -1, urlSection.Value);
                                // This could be refactored in a helper class for comics.org
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

                                            if (coverImage != null)
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
