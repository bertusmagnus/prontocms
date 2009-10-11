using System;
using System.Linq;
using Xunit;

namespace Pronto
{
    public class HtmlHelperTests
    {
        [Fact]
        public void Converts_nbsp()
        {
            Assert.Equal("<p>" + (char)160 + "</p>", "<p>&nbsp;</p>".ParseXObjects().First().ToString());
        }

        [Fact]
        public void Converts_pound()
        {
            Assert.Equal("<p>£</p>", "<p>&pound;</p>".ParseXObjects().First().ToString());
        }

        [Fact]
        public void Throws_when_malformed_entity()
        {
            Assert.Throws<ArgumentException>(() => "<p>&nsbp</p>".ParseXObjects());
        }

        [Fact]
        public void Numeric_entity()
        {
            Assert.Equal("<p>" + (char)160 + "</p>", "<p>&#160;</p>".ParseXObjects().First().ToString());
        }
    }
}
