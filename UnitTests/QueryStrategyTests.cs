using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class QueryStrategyTests
    {
        [TestMethod]
        public void GetParameterMatch_RequiredParametersNotAllPresent_ReturnsZero()
        {
            var mock = new QueryStrategyMock()
            {
                SetRequiredParameters = () => new [] { "param1", "param2", "param3" }
            };

            var matchRatio = mock.GetParameterMatch(new[] { ("param1", "value1"), ("param2", "value2") });

            Assert.AreEqual(0, matchRatio);
        }

        [TestMethod]
        public void GetParameterMatch_RequiredParametersAllPresent_Returns1()
        {
            var mock = new QueryStrategyMock()
            {
                SetRequiredParameters = () => new[] { "param1", "param2", }
            };

            var matchRatio = mock.GetParameterMatch(new[] { ("param1", "value1"), ("param2", "value2") });

            Assert.AreEqual(1, matchRatio);
        }

        [TestMethod]
        public void GetParameterMatch_MoreParametersThanRequired_Ratio()
        {
            var mock = new QueryStrategyMock()
            {
                SetRequiredParameters = () => new[] { "param1", "param2", }
            };

            var matchRatio = mock.GetParameterMatch(new[] { ("param1", "value1"), ("param2", "value2"), ("param3", "value3") });

            Assert.AreEqual(2/3f, matchRatio);
        }

        [TestMethod]
        public void GetParameterMatch_SameParamsDifferentCasing_Returns0()
        {
            var mock = new QueryStrategyMock()
            {
                SetRequiredParameters = () => new[] { "parAm1", "pAram2", }
            };

            var matchRatio = mock.GetParameterMatch(new[] { ("param1", "value1"), ("param2", "value2"), ("param3", "value3") });

            Assert.AreEqual(0, matchRatio);
        }
    }
}
