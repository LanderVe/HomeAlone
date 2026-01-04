using HomeAlone.Lights;
using System;
using Xunit;

namespace HomeAlone.Tests.Lights
{
    public class RelaisTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_ValidChannelId_CreatesRelais()
        {
            var relais = new Relais(3, 4);
            Assert.Equal(3, relais.ModuleId);
            Assert.Equal(4, relais.ChannelId);
        }

        [Fact]
        public void Constructor_ChannelIdZero_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Relais(3, 0));
            Assert.Equal("channelId", ex.ParamName);
        }

        [Fact]
        public void Constructor_MinimumValidChannelId_CreatesRelais()
        {
            var relais = new Relais(0, 1);
            Assert.Equal(0, relais.ModuleId);
            Assert.Equal(1, relais.ChannelId);
        }

        [Fact]
        public void Constructor_MaximumValidValues_CreatesRelais()
        {
            var relais = new Relais(byte.MaxValue, byte.MaxValue);
            Assert.Equal(byte.MaxValue, relais.ModuleId);
            Assert.Equal(byte.MaxValue, relais.ChannelId);
        }

        #endregion

        #region Parse Method Tests - Valid Inputs

        [Theory]
        [InlineData("3.2")]
        [InlineData(" 3.2")]
        [InlineData("3.2 ")]
        [InlineData("0.1")]
        [InlineData("255.255")]
        [InlineData("100.50")]
        public void Parse_ValidFormat_ReturnsCorrectRelais(string input)
        {
            var relais = Relais.Parse(input.AsSpan());
            
            var parts = input.Split('.');
            byte expectedModuleId = byte.Parse(parts[0]);
            byte expectedChannelId = byte.Parse(parts[1]);
            
            Assert.Equal(expectedModuleId, relais.ModuleId);
            Assert.Equal(expectedChannelId, relais.ChannelId);
        }

        [Fact]
        public void Parse_SimpleInput_3_2_ReturnsCorrectRelais()
        {
            var relais = Relais.Parse("3.2".AsSpan());
            Assert.Equal(3, relais.ModuleId);
            Assert.Equal(2, relais.ChannelId);
        }

        [Fact]
        public void Parse_MinimumValidValues_0_1_ReturnsCorrectRelais()
        {
            var relais = Relais.Parse("0.1".AsSpan());
            Assert.Equal(0, relais.ModuleId);
            Assert.Equal(1, relais.ChannelId);
        }

        [Fact]
        public void Parse_MaximumValidValues_255_255_ReturnsCorrectRelais()
        {
            var relais = Relais.Parse("255.255".AsSpan());
            Assert.Equal(255, relais.ModuleId);
            Assert.Equal(255, relais.ChannelId);
        }

        #endregion

        #region Parse Method Tests - Invalid Format

        [Theory]
        [InlineData("")]
        [InlineData("3")]
        [InlineData(".")]
        [InlineData("3.")]
        [InlineData(".2")]
        public void Parse_InvalidFormat_ThrowsFormatException(string input)
        {
            var ex = Assert.Throws<FormatException>(() => Relais.Parse(input.AsSpan()));
            Assert.NotNull(ex.Message);
        }


        [Fact]
        public void Parse_MissingModuleId_ThrowsFormatException()
        {
            var ex = Assert.Throws<FormatException>(() => Relais.Parse(".2".AsSpan()));
            Assert.Contains("Invalid", ex.Message);
        }

        [Fact]
        public void Parse_EmptyString_ThrowsFormatException()
        {
            var ex = Assert.Throws<FormatException>(() => Relais.Parse("".AsSpan()));
            Assert.NotNull(ex.Message);
        }

        #endregion

        #region Parse Method Tests - Invalid Values

        [Theory]
        [InlineData("256.1")]
        [InlineData("1.256")]
        [InlineData("300.300")]
        public void Parse_ValueOutOfByteRange_ThrowsFormatException(string input)
        {
            var ex = Assert.Throws<FormatException>(() => Relais.Parse(input));
            Assert.NotNull(ex.Message);
        }

        [Theory]
        [InlineData("abc.2")]
        [InlineData("3.xyz")]
        [InlineData("abc.xyz")]
        public void Parse_NonNumericValues_ThrowsFormatException(string input)
        {
            var ex = Assert.Throws<FormatException>(() => Relais.Parse(input));
            Assert.NotNull(ex.Message);
        }

        [Theory]
        [InlineData("-1.2")]
        [InlineData("3.-2")]
        public void Parse_NegativeValues_ThrowsFormatException(string input)
        {
            var ex = Assert.Throws<FormatException>(() => Relais.Parse(input));
            Assert.NotNull(ex.Message);
        }

        #endregion

        #region Parse Method Tests - Too Many Parts

        [Theory]
        [InlineData("3.2.1")]
        [InlineData("1.2.3.4")]
        [InlineData("3.2.1.4.5")]
        public void Parse_TooManyParts_ThrowsFormatException(string input)
        {
            var ex = Assert.Throws<FormatException>(() => Relais.Parse(input));
            Assert.Contains("Expected format", ex.Message);
        }

        #endregion

        #region Parse Method Tests - Edge Cases

        [Fact]
        public void Parse_LeadingZeros_ReturnsCorrectRelais()
        {
            var relais = Relais.Parse("003.002");
            Assert.Equal(3, relais.ModuleId);
            Assert.Equal(2, relais.ChannelId);
        }

        #endregion

        #region Parse Method Tests - Channel ID Validation

        [Fact]
        public void Parse_ChannelIdZero_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Relais.Parse("3.0"));
            Assert.Equal("channelId", ex.ParamName);
        }

        [Fact]
        public void Parse_ChannelIdOne_ReturnsValidRelais()
        {
            var relais = Relais.Parse("3.1");
            Assert.Equal(3, relais.ModuleId);
            Assert.Equal(1, relais.ChannelId);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Relais_EqualValues_AreEqual()
        {
            var relais1 = new Relais(3, 4);
            var relais2 = new Relais(3, 4);
            Assert.Equal(relais1, relais2);
        }

        [Fact]
        public void Relais_DifferentModuleId_AreNotEqual()
        {
            var relais1 = new Relais(3, 4);
            var relais2 = new Relais(4, 4);
            Assert.NotEqual(relais1, relais2);
        }

        [Fact]
        public void Relais_DifferentChannelId_AreNotEqual()
        {
            var relais1 = new Relais(3, 4);
            var relais2 = new Relais(3, 5);
            Assert.NotEqual(relais1, relais2);
        }

        [Fact]
        public void Parse_SameStringParsedTwice_ProduceEqualRelais()
        {
            var relais1 = Relais.Parse("3.2");
            var relais2 = Relais.Parse("3.2");
            Assert.Equal(relais1, relais2);
        }

        #endregion
    }
}
