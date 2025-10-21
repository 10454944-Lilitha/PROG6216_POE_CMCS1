using CMCS1.Controllers;
using CMCS1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CMCS1.Test.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public void SelectRole_ShouldReturnView()
        {
            // Arrange
            var controller = new AccountController();

            // Act
            var result = controller.SelectRole();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Theory]
        [InlineData("Lecturer")]
        [InlineData("Manager1")]
        [InlineData("Manager2")]
        public void SetRole_WithValidRole_ShouldSetSessionAndRedirect(string role)
        {
            // Arrange
            var controller = new AccountController();
            var httpContext = new DefaultHttpContext();
            var session = new MockHttpSession();
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.SetRole(role);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            
            // Verify session was set
            var sessionValue = session.GetString("UserRole");
            Assert.Equal(role, sessionValue);

            // Verify redirect based on role
            if (role == "Lecturer")
            {
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Claims", redirectResult.ControllerName);
            }
            else
            {
                Assert.Equal("ReviewClaims", redirectResult.ActionName);
                Assert.Equal("Claims", redirectResult.ControllerName);
            }
        }

        [Fact]
        public void SetRole_WithEmptyRole_ShouldRedirectToSelectRole()
        {
            // Arrange
            var controller = new AccountController();
            var httpContext = new DefaultHttpContext();
            var session = new MockHttpSession();
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.SetRole("");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SelectRole", redirectResult.ActionName);
        }

        [Fact]
        public void Logout_ShouldClearSessionAndRedirect()
        {
            // Arrange
            var controller = new AccountController();
            var httpContext = new DefaultHttpContext();
            var session = new MockHttpSession();
            session.SetString("UserRole", "Lecturer");
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SelectRole", redirectResult.ActionName);
            
            // Verify session was cleared
            var sessionValue = session.GetString("UserRole");
            Assert.Null(sessionValue);
        }
    }
}
