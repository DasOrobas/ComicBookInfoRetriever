namespace ComicBookInfoRetriever
{
    public class WeightedResult
    {
        public (string parameterNames, string parameterValues)[] parameters;

        public string ImageSource { get; set; }

        public bool Success { get; set; }  
    }
}
