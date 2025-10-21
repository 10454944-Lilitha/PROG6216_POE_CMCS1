using CMCS1.Controllers;
using CMCS1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CMCS1.Test.Controllers
{
    public class ClaimsControllerTests
    {
        private ClaimsController GetControllerWithSession(string role)
        {
            var controller = new ClaimsController();
            var httpContext = new DefaultHttpContext();
            var session = new MockHttpSession();
            session.SetString("UserRole", role);
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            return controller;
        }

        [Fact]
        public void Index_WithoutSession_ShouldRedirectToSelectRole()
        {
            // Arrange
            var controller = new ClaimsController();
            var httpContext = new DefaultHttpContext();
            var session = new MockHttpSession();
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SelectRole", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public void Index_WithLecturerRole_ShouldReturnView()
        {
            // Arrange
            var controller = GetControllerWithSession("Lecturer");

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void SubmitClaim_Get_WithLecturerRole_ShouldReturnView()
        {
            // Arrange
            var controller = GetControllerWithSession("Lecturer");

            // Act
            var result = controller.SubmitClaim();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void SubmitClaim_Get_WithManagerRole_ShouldRedirectToReviewClaims()
        {
            // Arrange
            var controller = GetControllerWithSession("Manager1");

            // Act
            var result = controller.SubmitClaim();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ReviewClaims", redirectResult.ActionName);
        }

        [Fact]
        public void ReviewClaims_WithManagerRole_ShouldReturnView()
        {
            // Arrange
            var controller = GetControllerWithSession("Manager1");

            // Act
            var result = controller.ReviewClaims();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ReviewClaims_WithLecturerRole_ShouldRedirectToIndex()
        {
            // Arrange
            var controller = GetControllerWithSession("Lecturer");

            // Act
            var result = controller.ReviewClaims();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }

    // Mock session class for testing
    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
    }
}
