namespace ComicBookInfoRetriever
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Linq;

    public abstract class QueryStrategy
    {
        public abstract string[] RequiredParameters { get; }
        public abstract Task<WeightedResult> Execute((string parameterName, string parameterValue)[] parameters, HttpClient httpClient);

        public virtual float GetParameterMatch((string parameterName, string parameterValue)[] testParameters)
        {
            float matchRatio = 0;

            if (!this.RequiredParameters.Except(testParameters.Select(x => x.parameterName)).Any())
            {
                matchRatio = (float)this.RequiredParameters.Length / (float)testParameters.Length;
            }

            return matchRatio;
        }
    }
}
