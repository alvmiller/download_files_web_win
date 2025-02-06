using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

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
            _browserHandler.ClearBrowserData();
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
            buttonExit.BackColor = Color.White;
            buttonClear.Enabled = false;
            buttonExit.Enabled = false;
            progressBar1.Show();
            _browserHandler.ClearBrowserData();
        }

        private void enableForExecute()
        {
            buttonTry.Enabled = true;
            buttonTry.BackColor = Color.ForestGreen;
            buttonExit.BackColor = Color.Crimson;
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
            try
            {
                bool isDone = false;
                disableForExecute();

                do
                {
                    if (!_linkValidator.IsValidLink(maskedTextBox1.Text))
                    {
                        break;
                    }

                    if (!_browserHandler.LoadPage(maskedTextBox1.Text))
                    {
                        break;
                    }

                    var audioCollection = _browserHandler.GetAudioElements();
                    if (audioCollection == null)
                    {
                        break;
                    }

                    await Task.Run(() =>
                    {
                        isDone = _fileDownloader.DownloadFiles(audioCollection);
                    });
                } while (false);

                if (isDone)
                {
                    MessageBox.Show("All Done!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Some error detected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                enableForExecute();
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

    public class LinkValidator
    {
        private const string DomainStr = "english-practice.net";

        public bool IsValidLink(string link)
        {
            _ = link ?? throw new ArgumentException("Parameter cannot be null", nameof(link));

            string urlStr = link.Trim();
            if (string.IsNullOrEmpty(urlStr))
            {
                return false;
            }

            Uri hostUri = new Uri(urlStr);
            string host = hostUri.Host;
            if (host != DomainStr)
            {
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

        public bool LoadPage(string url, bool isAsync = true)
        {
            _ = url ?? throw new ArgumentException("Parameter cannot be null", nameof(url));
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Parameter cannot be empty", nameof(url));
            }

            _webBrowser.Navigate(url);
            while (_webBrowser.ReadyState != WebBrowserReadyState.Complete)
            {
                if (isAsync)
                {
                    Application.DoEvents();
                }
            }
            if (_webBrowser.Document == null)
            {
                return false;
            }

            return true;
        }

        public HtmlElementCollection GetAudioElements()
        {
            var audioCollection = _webBrowser.Document.GetElementsByTagName("audio");
            if (audioCollection == null)
            {
                return null;
            }

            return audioCollection;
        }
    }

    public class FileDownloader
    {
        private const string FileExtension = ".mp3";
        private const char CharToTrim = '/';

        public bool DownloadFiles(HtmlElementCollection audioCollection)
        {
            _ = audioCollection ??
                throw new ArgumentException("Parameter cannot be null", nameof(audioCollection));
            
            bool isDownloaded = false;

            foreach (HtmlElement element in audioCollection)
            {
                if (element.GetAttribute("wp-audio-shortcode") == null)
                {
                    continue;
                }
                var srcElements = element.GetElementsByTagName("a");
                if (srcElements == null)
                {
                    continue;
                }
                foreach (HtmlElement src in srcElements)
                {
                    string srcElement = src.GetAttribute("href");
                    if (srcElement == null)
                    {
                        continue;
                    }

                    Uri filePath = new Uri(srcElement);
                    string fileFull = Path.GetFileName(filePath.LocalPath);
                    string fileSimplePath = fileFull.Trim(CharToTrim);
                    if (string.IsNullOrEmpty(fileSimplePath))
                    {
                        continue;
                    }
                    if (!fileSimplePath.EndsWith(FileExtension))
                    {
                        continue;
                    }

                    var client = new WebClient();
                    client.DownloadFile(filePath, fileSimplePath);
                    if (!File.Exists(fileSimplePath))
                    {
                        return false;
                    }
                    isDownloaded = true;
                }
            }

            return isDownloaded;
        }
    }
}