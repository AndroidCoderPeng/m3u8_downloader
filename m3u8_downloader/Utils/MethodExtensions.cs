using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using m3u8_downloader.Models;

namespace m3u8_downloader.Utils
{
    public static class MethodExtensions
    {
        private static readonly HttpClient Client = new HttpClient();

        private const string Agent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36 Edg/136.0.0.0";

        static MethodExtensions()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", Agent);
            Client.DefaultRequestHeaders.Add("Accept", "*/*");
            Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            Client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            Client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            Client.Timeout = TimeSpan.FromMinutes(5); // 设置为 5 分钟
        }

        private const string KeyPattern =
            @"#EXT-X-KEY:METHOD=(?<METHOD>[^,]*),URI=""(?<URI>[^""]*)""(?:,IV=(?<IV>[^,\s]*))?";

        private const string DurationPattern = @"#EXTINF:(\d+(\.\d+)?),";

        /// <summary>
        /// 解析m3u8基本信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<(List<string> Segments, double TotalDuration, Dictionary<string, string>)>
            ParseVideoResourceAsync(this string url)
        {
            var response = await Client.GetStringAsync(url);
            var lines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            double totalDuration = 0;
            var segments = new List<string>();
            foreach (var line in lines)
            {
                //解析时间
                var match = Regex.Match(line, DurationPattern);
                if (match.Success)
                {
                    var durationStr = match.Groups[1].Value;
                    totalDuration += double.Parse(durationStr);
                }

                //不带密钥的片段
                if (line.EndsWith(".ts"))
                {
                    segments.Add(new Uri(new Uri(url), line).ToString());
                }

                if (line.StartsWith("http") || line.StartsWith("https"))
                {
                    //带密钥的片段
                    segments.Add(line);
                }
            }

            //提取密钥和加密方式
            var dictionary = new Dictionary<string, string>();
            {
                var match = Regex.Match(response, KeyPattern);
                if (match.Success)
                {
                    var method = match.Groups["METHOD"].Value;
                    var uri = match.Groups["URI"].Value;
                    var iv = match.Groups["IV"].Success ? match.Groups["IV"].Value : "";
                    if (iv.StartsWith("0x"))
                    {
                        iv = iv.Substring(2);
                    }

                    dictionary.Add("Method", method);
                    dictionary.Add("URI", uri);
                    dictionary.Add("IV", iv);
                }
            }

            return (segments, totalDuration, dictionary);
        }

        /// <summary>
        /// 下载并解密加密的ts片段
        /// </summary>
        /// <param name="tsUrls"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="outputFolder"></param>
        /// <param name="progress"></param>
        public static async Task DownloadAndDecryptTsSegmentAsync(this List<string> tsUrls, byte[] key,
            byte[] iv, string outputFolder, IProgress<TaskProgress> progress)
        {
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var totalSegments = tsUrls.Count;
            long totalBytes = 0;
            var downloadedCount = 0;

            var downloadTasks = tsUrls.Select(async url =>
            {
                var fileName = Path.Combine(outputFolder, Path.GetFileName(url).Split('?')[0]); // 去除URL参数
                var retryCount = 3;

                while (retryCount-- > 0)
                {
                    try
                    {
                        var encryptedData = await Client.GetByteArrayAsync(url);
                        var decryptedData = encryptedData.DecryptAesCbc(key, iv);

                        using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write,
                                   FileShare.None, bufferSize: 4096, useAsync: true))
                        {
                            await fileStream.WriteAsync(decryptedData, 0, decryptedData.Length);
                        }

                        Interlocked.Add(ref totalBytes, encryptedData.Length);
                        var currentCount = Interlocked.Increment(ref downloadedCount);

                        progress.Report(new TaskProgress
                        {
                            TotalSegments = totalSegments,
                            DownloadedSegments = currentCount,
                            TotalBytes = totalBytes,
                            PercentComplete = (double)currentCount / totalSegments * 100
                        });

                        break;
                    }
                    catch (Exception ex) when (ex is IOException || ex is HttpRequestException)
                    {
                        Console.WriteLine($@"下载失败 {url}，剩余重试次数：{retryCount}，错误：{ex.Message}");
                        await Task.Delay(2000);
                    }
                }
            });

            await Task.WhenAll(downloadTasks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        private static byte[] DecryptAesCbc(this byte[] bytes, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                }
            }
        }

        /// <summary>
        /// 下载ts片段
        /// </summary>
        /// <param name="tsUrls"></param>
        /// <param name="outputFolder"></param>
        /// <param name="progress"></param>
        public static async Task DownloadTsSegmentsAsync(this List<string> tsUrls, string outputFolder,
            IProgress<TaskProgress> progress)
        {
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var totalSegments = tsUrls.Count;
            long totalBytes = 0;
            var downloadedCount = 0;

            var downloadTasks = tsUrls.Select(async url =>
            {
                var fileName = Path.Combine(outputFolder, Path.GetFileName(url));
                var retryCount = 3;

                while (retryCount-- > 0)
                {
                    try
                    {
                        var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode(); // 确保状态码是 2xx

                        var fileSize = response.Content.Headers.ContentLength ?? -1L;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = File.Create(fileName))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                        Interlocked.Add(ref totalBytes, fileSize);
                        var currentCount = Interlocked.Increment(ref downloadedCount);

                        progress.Report(new TaskProgress
                        {
                            TotalSegments = totalSegments,
                            DownloadedSegments = currentCount,
                            TotalBytes = totalBytes,
                            PercentComplete = (double)currentCount / totalSegments * 100
                        });

                        break; // 成功则退出循环
                    }
                    catch (Exception ex) when (ex is IOException || ex is HttpRequestException)
                    {
                        Console.WriteLine($@"下载失败 {url}，剩余重试次数：{retryCount}，错误：{ex.Message}");
                        await Task.Delay(2000); // 等待后重试
                    }
                }
            });

            await Task.WhenAll(downloadTasks);
        }

        /// <summary>
        /// 合并ts片段
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        public static async Task MergeTsSegmentsAsync(this string folder, string fileName)
        {
            var ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            var files = Directory.GetFiles(folder, "*.ts");
            if (!files.Any())
            {
                Console.WriteLine(@"没有找到任何ts文件");
                return;
            }

            var outputFile = Path.Combine(folder, $"{fileName}.mp4");
            var fileListFile = Path.Combine(folder, "filelist.txt");
            // 创建文件列表
            var builder = new StringBuilder();
            var sortedFiles = Directory.GetFiles(folder, "*.ts")
                .OrderBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase);

            foreach (var file in sortedFiles)
            {
                builder.AppendLine($"file '{file}'");
            }

            File.WriteAllText(fileListFile, builder.ToString());

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-f concat -safe 0 -i \"{fileListFile}\" -c copy \"{outputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                await Task.Run(() => process.WaitForExit());
            }

            // 删除临时文件列表
            File.Delete(fileListFile);
        }

        /// <summary>
        /// 删除文件夹下面所有的ts文件
        /// </summary>
        /// <param name="folder"></param>
        public static async Task DeleteTsSegments(this string folder)
        {
            var files = Directory.GetFiles(folder, "*.ts");
            await Task.Run(() => Parallel.ForEach(files, File.Delete));
        }

        public static byte[] GetByteArray(this string keyUrl)
        {
            return Client.GetByteArrayAsync(keyUrl).Result;
        }

        public static byte[] ToByteArray(this string value)
        {
            var numberChars = value.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}