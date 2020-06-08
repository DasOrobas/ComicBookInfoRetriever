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

                        var issue = issues.FirstOrDefault(x => x.InnerHtml.Contains(issueYear, StringComparison.InvariantCultureIgnoreCase));

                        if (issue != null)
                        {
                            weightedResult.Success = true;
                            weightedResult.parameters = parameters;
                            weightedResult.ImageSource = issue.Children.ElementAt(0).Children.ElementAt(0).Children.ElementAt(0).Attributes.ElementAt(0).Value;

                        }

                        return weightedResult;
                    }
                }
            }
        }
    }
}
