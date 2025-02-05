using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            string errMsg = "";
            string validLink = "https://english-practice.net/practice-english-listening-tests-for-b2-listening-test-19/";

            // Act
            bool result = validator.IsValidLink(validLink, ref errMsg);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("", errMsg);
        }

        [TestMethod]
        public void IsValidLink_InvalidLink_ReturnsFalse()
        {
            // Arrange
            var validator = new LinkValidator();
            string errMsg = "";
            string invalidLink = "https://invalid-domain.net/";

            // Act
            bool result = validator.IsValidLink(invalidLink, ref errMsg);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Bad Domain/Host Name", errMsg);
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

        [TestMethod]
        public void LoadPage_ValidUrl_ReturnsTrue()
        {
            // Arrange
            var handler = new BrowserHandler();
            string errMsg = "";
            string validUrl = "https://english-practice.net/practice-english-listening-tests-for-b2-listening-test-19/";

            // Act
            bool result = handler.LoadPage(validUrl, ref errMsg);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("", errMsg);
        }

        [TestMethod]
        public void LoadPage_InvalidUrl_ReturnsFalse()
        {
            // Arrange
            var handler = new BrowserHandler();
            string errMsg = "";
            string invalidUrl = "https://invalid-url/";

            // Act
            bool result = handler.LoadPage(invalidUrl, ref errMsg);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Bad Document from Browser", errMsg);
        }

        [TestMethod]
        public void GetAudioElements_ValidDocument_ReturnsAudioCollection()
        {
            // Arrange
            var handler = new BrowserHandler();
            string errMsg = "";
            string validUrl = "https://english-practice.net/practice-english-listening-tests-for-b2-listening-test-19/";
            handler.LoadPage(validUrl, ref errMsg);

            // Act
            var audioCollection = handler.GetAudioElements(ref errMsg);

            // Assert
            Assert.IsNotNull(audioCollection);
            Assert.AreEqual("", errMsg);
        }
    }

    [TestClass]
    public class FileDownloaderTests
    {
        //[TestMethod]
        //public void DownloadFiles_ValidAudioCollection_ReturnsTrue()
        //{
        //    // Arrange
        //    var downloader = new FileDownloader();
        //    string errMsg = "";
        //    var audioCollection = new HtmlElementCollection(null); // Mock or create a valid HtmlElementCollection

        //    // Act
        //    bool result = downloader.DownloadFiles(audioCollection, ref errMsg);

        //    // Assert
        //    Assert.IsTrue(result);
        //    Assert.AreEqual("", errMsg);
        //}

        [TestMethod]
        public void DownloadFiles_InvalidAudioCollection_ReturnsFalse()
        {
            // Arrange
            var downloader = new FileDownloader();
            string errMsg = "";
            HtmlElementCollection audioCollection = null;
            //var audioCollection = new HtmlElementCollection(null); // Mock or create an invalid HtmlElementCollection

            // Act
            bool result = downloader.DownloadFiles(audioCollection, ref errMsg);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Can't Find and/or Download any MP3 Media Audio File", errMsg);
        }
    }
}