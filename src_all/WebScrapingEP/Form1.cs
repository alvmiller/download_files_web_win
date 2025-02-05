using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

// Example pages:
// https://english-practice.net/practice-english-listening-tests-for-b2-listening-test-19/
// https://english-practice.net/practice-english-listening-tests-for-b2-listening-test-20/

namespace WebScrapingEP
{
    public partial class Form1 : Form
    {
        private readonly LinkValidator _linkValidator;
        private readonly BrowserHandler _browserHandler;
        private readonly FileDownloader _fileDownloader;

        public Form1()
        {
            InitializeComponent();
            progressBar1.Style = ProgressBarStyle.Marquee;
            _linkValidator = new LinkValidator();
            _browserHandler = new BrowserHandler();
            _fileDownloader = new FileDownloader();
        }

        private void clearForm()
        {
            progressBar1.Hide();
            maskedTextBox1.Text = "";
            _browserHandler.ClearBrowserData();
        }

        private void disableForExecute()
        {
            buttonTry.Enabled = false;
            buttonTry.BackColor = Color.White;
            buttonClear.Enabled = false;
            buttonExit.Enabled = false;
            progressBar1.Show();
        }

        private void enableForExecute()
        {
            buttonTry.Enabled = true;
            buttonTry.BackColor = Color.ForestGreen;
            buttonClear.Enabled = true;
            buttonExit.Enabled = true;
            progressBar1.Hide();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            clearForm();
        }

        private async void buttonTry_Click(object sender, EventArgs e)
        {
            string errorStr = "";
            bool isDone = false;
            disableForExecute();

            do
            {
                if (!_linkValidator.IsValidLink(maskedTextBox1.Text, ref errorStr))
                {
                    break;
                }

                if (!_browserHandler.LoadPage(maskedTextBox1.Text, ref errorStr))
                {
                    break;
                }

                var audioCollection = _browserHandler.GetAudioElements(ref errorStr);
                if (audioCollection == null)
                {
                    break;
                }

                bool isGot = false;
                await Task.Run(() =>
                {
                    if (!(isGot = _fileDownloader.DownloadFiles(audioCollection, ref errorStr)))
                    {
                        return;
                    }
                });
                if (!isGot)
                {
                    break;
                }

                isDone = true;
            } while (false);

            if (isDone)
            {
                MessageBox.Show("All Done!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(errorStr, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            enableForExecute();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

    public class LinkValidator
    {
        private const string DomainStr = "english-practice.net";

        public bool IsValidLink(string link, ref string errMsg)
        {
            string urlStr = link.Trim();
            if (string.IsNullOrEmpty(urlStr))
            {
                errMsg += "Empty Link";
                return false;
            }

            Uri hostUri = new Uri(urlStr);
            string host = hostUri.Host;
            if (host != DomainStr)
            {
                errMsg += "Bad Domain/Host Name";
                return false;
            }

            return true;
        }
    }

    public class BrowserHandler
    {
        private readonly WebBrowser _webBrowser;

        public BrowserHandler()
        {
            _webBrowser = new WebBrowser();
            _webBrowser.ScriptErrorsSuppressed = true;
            _webBrowser.Navigate("about:blank");
        }

        public void ClearBrowserData()
        {
            _webBrowser.Navigate("about:blank");
        }

        public bool LoadPage(string url, ref string errMsg)
        {
            _webBrowser.Navigate(url);
            while (_webBrowser.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }
            if (_webBrowser.Document == null)
            {
                errMsg += "Bad Document from Browser";
                return false;
            }

            return true;
        }

        public HtmlElementCollection GetAudioElements(ref string errMsg)
        {
            var audioCollection = _webBrowser.Document.GetElementsByTagName("audio");
            if (audioCollection == null)
            {
                errMsg += "Bad |audio| Value in Document";
                return null;
            }

            return audioCollection;
        }
    }

    public class FileDownloader
    {
        private const string FileExtension = ".mp3";
        private const char CharToTrim = '/';

        public bool DownloadFiles(HtmlElementCollection audioCollection, ref string errMsg)
        {
            bool isDownloaded = false;

            foreach (HtmlElement element in audioCollection)
            {
                if (element.GetAttribute("wp-audio-shortcode") == null)
                {
                    errMsg += "Bad |wp-audio-shortcode| Value in Document";
                    continue;
                }
                var srcElements = element.GetElementsByTagName("a");
                if (srcElements == null)
                {
                    errMsg += "Bad |a| Value in Document";
                    continue;
                }
                foreach (HtmlElement src in srcElements)
                {
                    string srcElement = src.GetAttribute("href");
                    if (srcElement == null)
                    {
                        errMsg += "Bad |href| Value in Document";
                        continue;
                    }

                    Uri filePath = new Uri(srcElement);
                    string fileFull = Path.GetFileName(filePath.LocalPath);
                    string fileSimplePath = fileFull.Trim(CharToTrim);
                    if (string.IsNullOrEmpty(fileSimplePath))
                    {
                        errMsg += "Bad Media File Value in Document";
                        continue;
                    }
                    if (!fileSimplePath.EndsWith(FileExtension))
                    {
                        errMsg += "Bad Media File Extension in Document";
                        continue;
                    }

                    var client = new WebClient();
                    client.DownloadFile(filePath, fileSimplePath);
                    if (!File.Exists(fileSimplePath))
                    {
                        errMsg += "Internal Error During Download";
                        return false;
                    }
                    isDownloaded = true;
                }
            }

            if (!isDownloaded)
            {
                errMsg += "Can't Find and/or Download any MP3 Media Audio File";
            }
            return isDownloaded;
        }
    }
}