using CMCS1.Models;

namespace CMCS1.Test.Integration
{
    public class ApprovalWorkflowTests
    {
        [Fact]
        public void Claim_WithNoApprovals_ShouldBePending()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 50.00m,
                Status = "Pending"
            };

            // Act & Assert
            Assert.Equal("Pending", claim.Status);
            Assert.False(claim.Manager1Approved);
            Assert.False(claim.Manager2Approved);
            Assert.False(claim.IsFinallyApproved);
        }

        [Fact]
        public void Claim_WithManager1Approval_ShouldBePendingApproval()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 50.00m,
                Status = "Pending"
            };

            // Act - Simulate Manager1 approval
            claim.Manager1Approved = true;
            claim.Manager1ApprovalDate = DateTime.Now;
            claim.Status = "Pending Approval";

            // Assert
            Assert.Equal("Pending Approval", claim.Status);
            Assert.True(claim.Manager1Approved);
            Assert.False(claim.Manager2Approved);
            Assert.False(claim.IsFinallyApproved);
            Assert.NotNull(claim.Manager1ApprovalDate);
        }

        [Fact]
        public void Claim_WithBothManagersApproved_ShouldBeApproved()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 50.00m,
                Status = "Pending"
            };

            // Act - Simulate both managers approving
            claim.Manager1Approved = true;
            claim.Manager1ApprovalDate = DateTime.Now;
            claim.Manager2Approved = true;
            claim.Manager2ApprovalDate = DateTime.Now;
            claim.Status = "Approved";

            // Assert
            Assert.Equal("Approved", claim.Status);
            Assert.True(claim.Manager1Approved);
            Assert.True(claim.Manager2Approved);
            Assert.True(claim.IsFinallyApproved);
            Assert.NotNull(claim.Manager1ApprovalDate);
            Assert.NotNull(claim.Manager2ApprovalDate);
        }

        [Fact]
        public void Claim_WithRejection_ShouldBeRejectedImmediately()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 50.00m,
                Status = "Pending"
            };

            // Act - Simulate rejection
            claim.Status = "Rejected";
            var feedback = new Feedback
            {
                FeedbackId = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                Message = "Hours not accurately reflected",
                ReviewedBy = "Manager1",
                ReviewDate = DateTime.Now
            };
            claim.FeedbackList.Add(feedback);

            // Assert
            Assert.Equal("Rejected", claim.Status);
            Assert.False(claim.IsFinallyApproved);
            Assert.Single(claim.FeedbackList);
            Assert.Equal("Hours not accurately reflected", claim.FeedbackList[0].Message);
        }

        [Fact]
        public void Claim_MultipleFeedback_ShouldStoreAll()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 50.00m
            };

            // Act - Add multiple feedbacks
            var feedback1 = new Feedback
            {
                FeedbackId = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                Message = "Needs more documentation",
                ReviewedBy = "Manager1",
                ReviewDate = DateTime.Now
            };

            var feedback2 = new Feedback
            {
                FeedbackId = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                Message = "Approved with minor concerns",
                ReviewedBy = "Manager2",
                ReviewDate = DateTime.Now.AddMinutes(5)
            };

            claim.FeedbackList.Add(feedback1);
            claim.FeedbackList.Add(feedback2);

            // Assert
            Assert.Equal(2, claim.FeedbackList.Count);
            Assert.Equal("Manager1", claim.FeedbackList[0].ReviewedBy);
            Assert.Equal("Manager2", claim.FeedbackList[1].ReviewedBy);
        }

        [Fact]
        public void Claim_ApprovalDates_ShouldBeTrackedSeparately()
        {
            // Arrange
            var claim = new Claim();
            var manager1ApprovalTime = DateTime.Now;
            var manager2ApprovalTime = DateTime.Now.AddHours(2);

            // Act
            claim.Manager1Approved = true;
            claim.Manager1ApprovalDate = manager1ApprovalTime;
            
            // Simulate time passing
            claim.Manager2Approved = true;
            claim.Manager2ApprovalDate = manager2ApprovalTime;

            // Assert
            Assert.NotEqual(claim.Manager1ApprovalDate, claim.Manager2ApprovalDate);
            Assert.True(claim.Manager2ApprovalDate > claim.Manager1ApprovalDate);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        public void Claim_IsFinallyApproved_VariousScenarios(
            bool manager1Approved, bool manager2Approved, bool expectedResult)
        {
            // Arrange
            var claim = new Claim
            {
                Manager1Approved = manager1Approved,
                Manager2Approved = manager2Approved
            };

            // Act
            var result = claim.IsFinallyApproved;

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
