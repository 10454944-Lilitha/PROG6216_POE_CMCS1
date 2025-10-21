using CMCS1.Models;

namespace CMCS1.Test.Models
{
    public class ClaimTests
    {
        [Fact]
        public void Claim_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var claim = new Claim();

            // Assert
            Assert.NotEqual(Guid.Empty, claim.ClaimId);
            Assert.Equal("Pending", claim.Status);
            Assert.False(claim.Manager1Approved);
            Assert.False(claim.Manager2Approved);
            Assert.Null(claim.Manager1ApprovalDate);
            Assert.Null(claim.Manager2ApprovalDate);
            Assert.Empty(claim.FeedbackList);
            Assert.Empty(claim.UploadedFileNames);
        }

        [Fact]
        public void TotalAmount_ShouldCalculateCorrectly()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 50.00m
            };

            // Act
            var total = claim.TotalAmount;

            // Assert
            Assert.Equal(500.00m, total);
        }

        [Theory]
        [InlineData(1, 100.00, 100.00)]
        [InlineData(40, 25.50, 1020.00)]
        [InlineData(160, 75.00, 12000.00)]
        public void TotalAmount_WithVariousInputs_ShouldCalculateCorrectly(int hours, decimal rate, decimal expectedTotal)
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = hours,
                HourlyRate = rate
            };

            // Act
            var total = claim.TotalAmount;

            // Assert
            Assert.Equal(expectedTotal, total);
        }

        [Fact]
        public void IsFinallyApproved_WhenBothManagersApproved_ShouldReturnTrue()
        {
            // Arrange
            var claim = new Claim
            {
                Manager1Approved = true,
                Manager2Approved = true
            };

            // Act
            var isApproved = claim.IsFinallyApproved;

            // Assert
            Assert.True(isApproved);
        }

        [Fact]
        public void IsFinallyApproved_WhenOnlyManager1Approved_ShouldReturnFalse()
        {
            // Arrange
            var claim = new Claim
            {
                Manager1Approved = true,
                Manager2Approved = false
            };

            // Act
            var isApproved = claim.IsFinallyApproved;

            // Assert
            Assert.False(isApproved);
        }

        [Fact]
        public void IsFinallyApproved_WhenOnlyManager2Approved_ShouldReturnFalse()
        {
            // Arrange
            var claim = new Claim
            {
                Manager1Approved = false,
                Manager2Approved = true
            };

            // Act
            var isApproved = claim.IsFinallyApproved;

            // Assert
            Assert.False(isApproved);
        }

        [Fact]
        public void IsFinallyApproved_WhenNeitherManagerApproved_ShouldReturnFalse()
        {
            // Arrange
            var claim = new Claim
            {
                Manager1Approved = false,
                Manager2Approved = false
            };

            // Act
            var isApproved = claim.IsFinallyApproved;

            // Assert
            Assert.False(isApproved);
        }

        [Fact]
        public void Claim_WithFeedback_ShouldStoreFeedbackCorrectly()
        {
            // Arrange
            var claim = new Claim();
            var feedback = new Feedback
            {
                FeedbackId = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                Message = "Test feedback",
                ReviewedBy = "Manager1",
                ReviewDate = DateTime.Now
            };

            // Act
            claim.FeedbackList.Add(feedback);

            // Assert
            Assert.Single(claim.FeedbackList);
            Assert.Equal("Test feedback", claim.FeedbackList[0].Message);
            Assert.Equal("Manager1", claim.FeedbackList[0].ReviewedBy);
        }
    }
}
