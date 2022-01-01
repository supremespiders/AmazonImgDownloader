using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AmazonImgDownloader.Models;
using HtmlAgilityPack;

namespace AmazonImgDownloader.Helpers
{
    public static class Extensions
    {
        public static async Task<string> HandleAndRepeat(this HttpClient httpClient, HttpRequestMessage req, int maxAttempts = 1, CancellationToken ct = new CancellationToken())
        {
            int tries = 0;
            do
            {
                try
                {
                    var r = await httpClient.SendAsync(req, ct).ConfigureAwait(false);
                    var s = await r.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (s);
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = await new StreamReader(ex.Response.GetResponseStream()).ReadToEndAsync();
                    }
                    catch (Exception)
                    {
                        //
                    }

                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new KnownException($"Error calling : {req.RequestUri}\n{ex.Message} {errorMessage}");
                    }

                    await Task.Delay(2000, ct).ConfigureAwait(false);
                }
            } while (true);
        }

        public static async Task<string> PostJson(this HttpClient httpClient, string url, string json, int maxAttempts = 1, Dictionary<string, string> headers = null, CancellationToken ct = new CancellationToken())
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            if (content.Headers.ContentType != null)
                content.Headers.ContentType.CharSet = "";
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

            if (headers == null)
                return await httpClient.HandleAndRepeat(req, maxAttempts, ct);

            foreach (var header in headers)
                req.Headers.Add(header.Key, header.Value);

            return await httpClient.HandleAndRepeat(req, maxAttempts, ct);
        }

        public static async Task<string> PostFormData(this HttpClient httpClient, string url, Dictionary<string, string> data, int maxAttempts = 1, Dictionary<string, string> headers = null, CancellationToken ct = new CancellationToken())
        {
            var content = new FormUrlEncodedContent(data);
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

            if (headers == null)
                return await httpClient.HandleAndRepeat(req, maxAttempts, ct);

            foreach (var header in headers)
                req.Headers.Add(header.Key, header.Value);

            return await httpClient.HandleAndRepeat(req, maxAttempts, ct);
        }

        public static async Task DownloadFile(this HttpClient httpClient, string url, string localPath)
        {
            var response = await httpClient.GetAsync(url);
            using (var fs = new FileStream(localPath, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }
        }
        public static async Task<string> GetHtml(this HttpClient httpClient, string url, int maxAttempts = 1, Dictionary<string, string> headers = null, CancellationToken ct = new CancellationToken())
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);

            if (headers == null)
                return await httpClient.HandleAndRepeat(req, maxAttempts, ct);

            foreach (var header in headers)
                req.Headers.Add(header.Key, header.Value);

            return await httpClient.HandleAndRepeat(req, maxAttempts, ct);
        }

        public static HtmlDocument ToDoc(this string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
        public static string GetStringBetween(this string text, string start, string end)
        {
            var p1 = text.IndexOf(start, StringComparison.Ordinal) + start.Length;
            if (p1 == start.Length - 1) return null;
            var p2 = text.IndexOf(end, p1, StringComparison.Ordinal);
            if (p2 == -1) return null;
            return end == "" ? text.Substring(p1) : text.Substring(p1, p2 - p1);
        }
        
       
    }
}