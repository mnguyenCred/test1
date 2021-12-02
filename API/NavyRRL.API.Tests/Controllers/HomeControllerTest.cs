using Microsoft.VisualStudio.TestTools.UnitTesting;

using NavyRRL.API;
using NavyRRL.API.Controllers;

using System.Web.Mvc;

namespace NavyRRL.API.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull( result );
            Assert.AreEqual( "Home Page", result.ViewBag.Title );
        }
    }
}
