using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using WebScrapingEP;

namespace WebScrapingEP.Tests
{
    [TestClass]
    public class LinkValidatorTests
    {
        [TestMethod]
        public void IsValidLink_ValidLink_ReturnsTrue()
        {
            // Arrange
            var validator = new LinkValidator();
            string validLink = "https://english-practice.net/practice-english-listening-tests-for-b2-listening-test-19/";

            // Act
            bool result = validator.IsValidLink(validLink);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidLink_InvalidLink_ReturnsFalse()
        {
            // Arrange
            var validator = new LinkValidator();
            string invalidLink = "https://invalid-domain.net/";

            // Act
            bool result = validator.IsValidLink(invalidLink);

            // Assert
            Assert.IsFalse(result);
        }
    }

    [TestClass]
    public class BrowserHandlerTests
    {
        private WebBrowser webBrowser;

        [TestInitialize]
        public void Setup()
        {
            webBrowser = new WebBrowser();
        }
    }

    [TestClass]
    public class FileDownloaderTests
    {
    }
}