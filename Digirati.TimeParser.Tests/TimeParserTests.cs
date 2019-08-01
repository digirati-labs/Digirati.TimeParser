using FluentAssertions;
using Xunit;

namespace Digirati.TimeParser.Tests
{
    public class TimeParserTests
    {
        [Theory]
        // Valid:
        [InlineData("1d", true)]
        [InlineData("1d 1h", true)]
        [InlineData("1d 1h 1m", true)]
        [InlineData("1d 1h 1m 1s", true)]
        [InlineData("1m 1h 1s", true)]
        [InlineData("1h 1m", true)]
        [InlineData("1d 1h 1s", true)]
        [InlineData("1d 1m 1s", true)]
        [InlineData("1h 1s", true)]
        [InlineData("1d 1s", true)]
        [InlineData("1d 1m", true)]
        [InlineData("1s", true)]
        [InlineData("0s", true)]
        [InlineData("0m 0s", true)]
        [InlineData("0h 0m 0s", true)]
        [InlineData("0d 0h 0m 0s", true)]
        [InlineData("1d 0h 0m 0s", true)]
        [InlineData("0d 1h 0m 0s", true)]
        [InlineData("0d 0h 1m 0s", true)]
        [InlineData("0d 0h 0m 1s", true)]
        [InlineData("1.5d", true)]
        [InlineData("1.5d 1.5h", true)]
        [InlineData("1.5d 1.5h 1.5m", true)]
        [InlineData("1.5d 1.5h 1.5m 1.5s", true)]
        [InlineData("1.75d", true)]
        [InlineData("1.275d", true)]
        [InlineData("1.5275d", true)]
        [InlineData("1.5275d 0.275235h 30m 0s", true)]
        [InlineData("1d1h1m1s", true)]
        [InlineData("1d1h1m1 s", true)]
        [InlineData("1d1h1 m1s", true)]
        [InlineData("1d1h 1 m1s", true)]
        [InlineData("1d1h 1 m               1s", true)]
        [InlineData(@"1d1h 


1 m               1

s", true)]
        // Invalid:
        [InlineData("",false)]
        [InlineData((string)null,false)]
        [InlineData("fnord",false)]
        [InlineData("25",false)]
        [InlineData("???",false)]
        [InlineData("1hh",false)]
        [InlineData("1mm",false)]
        [InlineData("1ss",false)]
        [InlineData("1h 1d 3",false)]
        [InlineData("1h 1d 3q",false)]
        [InlineData("1h 1d 3ss",false)]
        [InlineData("-3h",false)]
        [InlineData("15ms",false)]
        [InlineData("1+1m",false)]
        [InlineData("1h+1m",false)]
        private void TryParseTimeString_Expected_Success(string s, bool success)
        {
            TimeParser.TryParseTimeString(s, out _).Should().Be(success, "it has been manually determined to be so");
        }

        [Theory]
        [InlineData(@"1d1h 


1 m               1

s", (24*60*60+60*60+60+1))]
        [InlineData("1s", 1)]
        [InlineData("1m", 60)]
        [InlineData("10m", 10*60)]
        [InlineData("120m", 120*60)]
        [InlineData("1h", 60*60)]
        [InlineData("1d", 24*60*60)]
        private void ParseTimeString_Expected_Result(string s, int seconds)
        {
            TimeParser.ParseTimeString(s).TotalSeconds.Should().Be(seconds, "it has been manually calculated");
        }
    }
}