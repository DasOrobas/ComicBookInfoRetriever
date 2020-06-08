using ComicBookInfoRetriever;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class QueryStrategyMock : ComicBookInfoRetriever.QueryStrategy
    {
        public Func<string[]> SetRequiredParameters;

        public override string[] RequiredParameters => this.SetRequiredParameters();

        public override Task<WeightedResult> Execute((string parameterName, string parameterValue)[] parameters, HttpClient httpClient)
        {
            throw new NotImplementedException();
        }
    }
}
