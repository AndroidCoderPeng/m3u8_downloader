using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using m3u8_downloader.Models;

namespace m3u8_downloader.Utils
{
    public static class MethodExtensions
    {
        /// <summary>
        /// 提取html里面的m3u8资源
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ExtractM3U8Resource(this string html)
        {
            return "";
        }

        /// <summary>
        /// 解析m3u8基本信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<(List<string> Segments, double TotalDuration)> ParseVideoResourceAsync(this string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var reader = new StreamReader(await response.Content.ReadAsStreamAsync());

            var segments = new List<string>();
            double totalDuration = 0;
            string line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("#EXTINF:"))
                {
                    var durationStr = line.Replace("#EXTINF:", "").Split(',')[0];
                    totalDuration += double.Parse(durationStr);
                }
                else if (line.EndsWith(".ts"))
                {
                    segments.Add(new Uri(new Uri(url), line).ToString());
                }
            }

            return (segments, totalDuration);
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
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

                            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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
                                PercentComplete = (int)((double)currentCount / totalSegments * 100)
                            });

                            break; // 成功则退出循环
                        }
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
            const string ffmpeg = @"D:\Dev\ffmpeg\bin\ffmpeg.exe";
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
            foreach (var file in files.OrderBy(Path.GetFileName))
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
    }
}