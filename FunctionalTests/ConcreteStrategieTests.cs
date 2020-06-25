using ComicBookInfoRetriever;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests
{
    public class ConcreteStrategieTests
    {
        private static HttpClient testClient = new HttpClient();
        
        [Fact]
        public async Task NormalSearchSuccessfulCase()
        {
           
           ComicsDatabaseNormalSearch searchStrategy = new ComicsDatabaseNormalSearch();

           var result = await searchStrategy.Execute(new[] { ("issueYear", "1984"), ("issueNumber", "1"), ("seriesTitle", "Teenage Mutant Ninja Turtles") }, testClient);

            Assert.True(result.Success);
            Assert.Equal("https://files1.comics.org//img/gcd/covers_by_id/226/w200/226893.jpg?768706070855447727", result.ImageSource);


        }
    }
}
