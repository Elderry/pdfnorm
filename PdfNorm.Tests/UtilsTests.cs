using PdfNorm.Common;

namespace PdfNorm.Tests;

public class UtilsTests
{
    [Fact]
    public void CanBeTrimmed_ReturnsTrueForLeadingWhitespace()
    {
        // Arrange
        string input = "  test";

        // Act
        bool result = Utils.CanBeTrimmed(input);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanBeTrimmed_ReturnsTrueForTrailingWhitespace()
    {
        // Arrange
        string input = "test  ";

        // Act
        bool result = Utils.CanBeTrimmed(input);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanBeTrimmed_ReturnsFalseForNoWhitespace()
    {
        // Arrange
        string input = "test";

        // Act
        bool result = Utils.CanBeTrimmed(input);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Trim_RemovesLeadingAndTrailingWhitespace()
    {
        // Arrange
        string input = "  test  ";

        // Act
        string result = Utils.Trim(input);

        // Assert
        Assert.Equal("test", result);
    }
}
