using PdfNorm.Services;

namespace PdfNorm.Tests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void CanBeTrimmed_ReturnsTrueForLeadingWhitespace()
        {
            // Arrange
            string input = "  test";

            // Act
            bool result = TextUtils.CanBeTrimmed(input);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanBeTrimmed_ReturnsTrueForTrailingWhitespace()
        {
            // Arrange
            string input = "test  ";

            // Act
            bool result = TextUtils.CanBeTrimmed(input);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanBeTrimmed_ReturnsFalseForNoWhitespace()
        {
            // Arrange
            string input = "test";

            // Act
            bool result = TextUtils.CanBeTrimmed(input);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Trim_RemovesLeadingAndTrailingWhitespace()
        {
            // Arrange
            string input = "  test  ";

            // Act
            string result = TextUtils.Trim(input);

            // Assert
            Assert.AreEqual("test", result);
        }
    }
}
