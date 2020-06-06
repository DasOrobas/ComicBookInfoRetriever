using Microsoft.AspNetCore.Mvc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace ComicBookInfoRetriever
{
    public class ComicsDatabaseAdvancedQuery : IQueryStrategy
    {
        public WeightedResult Execute((string parameterName, string parameterValue)[] parameters)
        {
            WeightedResult weightedResult = new WeightedResult();

            weightedResult.IssueNumber = 1.0f;
            weightedResult.SeriesName = 1.0f;
            weightedResult.Year = 1.0f;

            string url = $"https://www.comics.org/search/advanced/process/?target=issue_cover&method=icontains&logic=False&keywords=&title=&feature=&job_number=&pages=&pages_uncertain=&script=&pencils=&inks=&colors=&letters=&story_editing=&first_line=&characters=&synopsis=&reprint_notes=&story_reprinted=&notes=&issues={weightedResult.IssueNumber}&volume=&issue_title=&variant_name=&is_variant=&issue_date=&indicia_frequency=&price=&issue_pages=&issue_pages_uncertain=&issue_editing=&isbn=&barcode=&rating=&issue_notes=&issue_reprinted=&is_indexed=&in_selected_collection=on&order1=series&order2=date&order3=&start_date=&end_date=&updated_since=&pub_name=&pub_notes=&brand_group=&brand_emblem=&brand_notes=&indicia_publisher=&is_surrogate=&ind_pub_notes=&series={parameters.SeriesName.Replace(" ", "+")}&series_year_began=&series_notes=&tracking_notes=&issue_count=&is_comics=&color=&dimensions=&paper_stock=&binding=&publishing_format=";



            return weightedResult;
        }

        public float GetParameterMatch((string parameterName, string parameterValue)[] parameters)
        {
            float matchRatio = 0;
            if(parameters.Any(x => x.parameterName == "IssueNumber") && parameters.Any(x => x.parameterName == "SeriesName") && parameters.Any(x => x.parameterName == "Year"))
            {
                matchRatio = 1;
            }

            return matchRatio;
        }
    }
}
