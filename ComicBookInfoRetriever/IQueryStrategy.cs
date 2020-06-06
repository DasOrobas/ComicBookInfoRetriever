namespace ComicBookInfoRetriever
{
    interface IQueryStrategy
    {
        WeightedResult Execute((string parameterName, string parameterValue)[] parameters);

        float GetParameterMatch((string parameterName, string parameterValue)[] parameters);
    }
}
