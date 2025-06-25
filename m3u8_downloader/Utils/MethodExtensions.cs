using System;
using System.Collections.Concurrent;
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
        private static readonly string _ffprobe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffprobe.exe");
        private static readonly string _ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
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
        public static async Task<ConcurrentDictionary<int, string>> DownloadAndDecryptTsSegmentAsync(
            this List<string> tsUrls, byte[] key, byte[] iv, string outputFolder, IProgress<TaskProgress> progress)
        {
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var totalSegments = tsUrls.Count;
            long totalBytes = 0;
            var downloadedCount = 0;

            // 使用 ConcurrentDictionary 来存储带索引的文件路径
            var indexedFiles = new ConcurrentDictionary<int, string>();

            var downloadTasks = tsUrls.Select(async (url, index) =>
            {
                var fileName = Path.Combine(outputFolder, Path.GetFileName(url).Split('?')[0]); // 去除URL参数
                var retryCount = 5;

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

                        // 将文件路径和索引存入 ConcurrentDictionary
                        indexedFiles.TryAdd(index, fileName);

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

            return indexedFiles;
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
        /// <param name="token"></param>
        public static async Task<ConcurrentDictionary<int, string>> DownloadTsSegmentsAsync(
            this List<string> tsUrls, string outputFolder, IProgress<TaskProgress> progress, CancellationToken token)
        {
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var totalSegments = tsUrls.Count;
            long totalBytes = 0;
            var downloadedCount = 0;

            // 使用 ConcurrentDictionary 来存储带索引的文件路径
            var indexedFiles = new ConcurrentDictionary<int, string>();

            var downloadTasks = tsUrls.Select(async (url, index) =>
            {
                // 检查是否已取消
                if (token.IsCancellationRequested) return;

                var fileName = Path.Combine(outputFolder, Path.GetFileName(url));
                var retryCount = 5;

                while (retryCount-- > 0)
                {
                    // 再次检查是否取消
                    if (token.IsCancellationRequested) return;

                    try
                    {
                        var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
                        response.EnsureSuccessStatusCode(); // 确保状态码是 2xx

                        var fileSize = response.Content.Headers.ContentLength ?? -1L;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = File.Create(fileName))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                        // 将文件路径和索引存入 ConcurrentDictionary
                        indexedFiles.TryAdd(index, fileName);

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
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        Console.WriteLine(@"下载已取消");
                        return;
                    }
                    catch (Exception ex) when (ex is IOException || ex is HttpRequestException)
                    {
                        Console.WriteLine($@"下载失败 {url}，剩余重试次数：{retryCount}，错误：{ex.Message}");
                        await Task.Delay(2000, token); // 等待后重试
                    }
                }
            });

            await Task.WhenAll(downloadTasks);

            return indexedFiles;
        }

        /// <summary>
        /// 合并ts片段  
        /// </summary>
        /// <param name="indexedFiles"></param>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <param name="totalDuration"></param>
        public static async Task MergeTsSegmentsAsync(this ConcurrentDictionary<int, string> indexedFiles,
            string folder, string fileName, double totalDuration, IProgress<double> progress)
        {
            if (!indexedFiles.Any())
            {
                Console.WriteLine(@"没有找到任何ts文件");
                return;
            }

            var outputFile = Path.Combine(folder, $"{fileName}.mp4");
            var fileListFile = Path.Combine(folder, "filelist.txt");

            // 按照索引顺序生成文件列表
            var builder = new StringBuilder();
            foreach (var keyValuePair in indexedFiles.OrderBy(kv => kv.Key))
            {
                builder.AppendLine($"file '{keyValuePair.Value}'");
            }

            File.WriteAllText(fileListFile, builder.ToString());

            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpeg,
                Arguments =
                    $"-hide_banner -nostats -loglevel warning -f concat -safe 0 -i \"{fileListFile}\" -c copy \"{outputFile}\" -progress pipe:1",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                // 读取进度输出
                await ReadProgressAsync(process.StandardOutput, totalDuration, progress);
                await Task.Run(() => process.WaitForExit());
            }

            // 删除临时文件列表
            File.Delete(fileListFile);
        }

        private static async Task ReadProgressAsync(StreamReader reader, double totalDuration,
            IProgress<double> progress)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("out_time="))
                {
                    var timeString = line.Replace("out_time=", "").Trim();
                    if (!timeString.StartsWith("-")) //去掉ffmpeg开始处理时的占位时间
                    {
                        var timeSpan = TimeSpan.Parse(timeString);
                        var currentDuration = (int)timeSpan.TotalSeconds;
                        var percent = Math.Min(100.0, currentDuration / totalDuration * 100.0);
                        progress.Report(percent);
                    }
                }
            }
        }

        /// <summary>
        /// 获取时长
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMediaDuration(this string filePath)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffprobe,
                        Arguments =
                            $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (double.TryParse(output.Trim(), out var durationSeconds))
                    {
                        return TimeSpan.FromSeconds(durationSeconds).ToString(@"hh\:mm\:ss");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"获取视频片段时长失败: {ex.Message}");
            }

            return "未知";
        }

        /// <summary>
        /// 获取视频分辨率
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetVideoResolution(this string filePath)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffprobe,
                        Arguments =
                            $"-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 \"{filePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return !string.IsNullOrWhiteSpace(output) ? output.Trim() : "未知";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"获取视频分辨率失败: {ex.Message}");
            }

            return "未知";
        }

        /// <summary>
        /// 生成视频封面图
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cacheFolderPath"></param>
        /// <returns></returns>
        public static string GenerateCoverImage(this string filePath, string cacheFolderPath)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var coverPath = Path.Combine(cacheFolderPath, $"{fileName}.jpg");
                if (File.Exists(coverPath))
                {
                    return coverPath;
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpeg,
                        Arguments = $"-i \"{filePath}\" -ss 00:00:01.000 -vframes 1 \"{coverPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                return File.Exists(coverPath) ? coverPath : null;
            }
            catch (Exception)
            {
                return null;
            }
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