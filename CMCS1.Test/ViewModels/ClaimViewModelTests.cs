using CMCS1.Models.ViewModels;

namespace CMCS1.Test.ViewModels
{
    public class ClaimViewModelTests
    {
        [Fact]
        public void ClaimViewModel_TotalAmount_ShouldCalculateCorrectly()
        {
            // Arrange
            var viewModel = new ClaimViewModel
            {
                HoursWorked = 20,
                HourlyRate = 45.50m
            };

            // Act
            var total = viewModel.TotalAmount;

            // Assert
            Assert.Equal(910.00m, total);
        }

        [Theory]
        [InlineData(0, 50.00, 0.00)]
        [InlineData(10, 0, 0.00)]
        [InlineData(5, 100.00, 500.00)]
        [InlineData(40, 30.75, 1230.00)]
        public void ClaimViewModel_TotalAmount_WithVariousInputs_ShouldCalculateCorrectly(
            int hours, decimal rate, decimal expectedTotal)
        {
            // Arrange
            var viewModel = new ClaimViewModel
            {
                HoursWorked = hours,
                HourlyRate = rate
            };

            // Act
            var total = viewModel.TotalAmount;

            // Assert
            Assert.Equal(expectedTotal, total);
        }

        [Fact]
        public void ClaimViewModel_Properties_ShouldSetCorrectly()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var notes = "Test notes";

            // Act
            var viewModel = new ClaimViewModel
            {
                ClaimId = claimId,
                HoursWorked = 15,
                HourlyRate = 60.00m,
                Notes = notes
            };

            // Assert
            Assert.Equal(claimId, viewModel.ClaimId);
            Assert.Equal(15, viewModel.HoursWorked);
            Assert.Equal(60.00m, viewModel.HourlyRate);
            Assert.Equal(notes, viewModel.Notes);
            Assert.Equal(900.00m, viewModel.TotalAmount);
        }

        [Fact]
        public void ClaimViewModel_Notes_CanBeNull()
        {
            // Arrange & Act
            var viewModel = new ClaimViewModel
            {
                HoursWorked = 10,
                HourlyRate = 50.00m,
                Notes = null
            };

            // Assert
            Assert.Null(viewModel.Notes);
        }
    }
}
