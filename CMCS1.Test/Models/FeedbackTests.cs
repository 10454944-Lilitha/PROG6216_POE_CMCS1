using CMCS1.Models;

namespace CMCS1.Test.Models
{
    public class FeedbackTests
    {
        [Fact]
        public void Feedback_ShouldInitializeWithCorrectValues()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var claimId = Guid.NewGuid();
            var message = "Excellent work!";
            var reviewedBy = "Manager1";
            var reviewDate = DateTime.Now;

            // Act
            var feedback = new Feedback
            {
                FeedbackId = feedbackId,
                ClaimId = claimId,
                Message = message,
                ReviewedBy = reviewedBy,
                ReviewDate = reviewDate
            };

            // Assert
            Assert.Equal(feedbackId, feedback.FeedbackId);
            Assert.Equal(claimId, feedback.ClaimId);
            Assert.Equal(message, feedback.Message);
            Assert.Equal(reviewedBy, feedback.ReviewedBy);
            Assert.Equal(reviewDate, feedback.ReviewDate);
        }

        [Fact]
        public void Feedback_Message_ShouldAcceptLongText()
        {
            // Arrange
            var longMessage = new string('A', 1000);
            var feedback = new Feedback
            {
                Message = longMessage
            };

            // Act & Assert
            Assert.Equal(1000, feedback.Message.Length);
            Assert.Equal(longMessage, feedback.Message);
        }

        [Theory]
        [InlineData("Manager1")]
        [InlineData("Manager2")]
        [InlineData("System")]
        public void Feedback_ReviewedBy_ShouldAcceptDifferentReviewers(string reviewer)
        {
            // Arrange & Act
            var feedback = new Feedback
            {
                ReviewedBy = reviewer
            };

            // Assert
            Assert.Equal(reviewer, feedback.ReviewedBy);
        }
    }
}
