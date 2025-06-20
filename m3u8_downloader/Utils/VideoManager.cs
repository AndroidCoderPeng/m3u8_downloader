using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using m3u8_downloader.Models;
using Newtonsoft.Json;

namespace m3u8_downloader.Utils
{
    public class VideoManager
    {
        private readonly string _ffprobe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffprobe.exe");
        private readonly string _ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
        private readonly string _videoFolderPath;
        private readonly string _cacheFolderPath;
        private readonly ConcurrentDictionary<string, VideoFile> _memoryCache;

        public VideoManager(string videoFolderPath)
        {
            _videoFolderPath = videoFolderPath;
            _cacheFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
            _memoryCache = new ConcurrentDictionary<string, VideoFile>();

            // 确保缓存文件夹存在
            Directory.CreateDirectory(_cacheFolderPath);

            // 加载现有缓存文件到内存
            Task.Run(LoadCacheFiles);
        }

        private void LoadCacheFiles()
        {
            try
            {
                if (!Directory.Exists(_cacheFolderPath))
                    return;

                var cacheFiles = Directory.GetFiles(_cacheFolderPath, "*.json");
                foreach (var cacheFile in cacheFiles)
                {
                    try
                    {
                        using (var stream = new FileStream(cacheFile, FileMode.Open, FileAccess.Read, FileShare.Read,
                                   4096))
                        using (var reader = new StreamReader(stream))
                        {
                            var json = reader.ReadToEnd();
                            var videoFile = JsonConvert.DeserializeObject<VideoFile>(json);
                            if (videoFile != null && File.Exists(videoFile.FilePath))
                            {
                                _memoryCache[Path.GetFileName(videoFile.FilePath)] = videoFile;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"加载缓存文件失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载缓存文件夹失败: {ex.Message}");
            }
        }

        public async Task<List<VideoFile>> GetVideosAsync()
        {
            try
            {
                var files = Directory.GetFiles(_videoFolderPath, "*.mp4");
                var tasks = files.Select(file => GetVideoByFileNameAsync(Path.GetFileName(file)));
                var results = await Task.WhenAll(tasks);
                return results.Where(v => v != null).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载视频时发生错误: {ex.Message}");
                return new List<VideoFile>();
            }
        }

        // 异步根据文件名获取视频信息
        private async Task<VideoFile> GetVideoByFileNameAsync(string fileName)
        {
            // 1. 检查内存缓存（快速路径）
            if (_memoryCache.TryGetValue(fileName, out var videoFile))
            {
                var filePath = Path.Combine(_videoFolderPath, fileName);
                if (File.Exists(filePath) && File.GetLastWriteTime(filePath) == videoFile.LastModified)
                {
                    return videoFile;
                }
            }

            // 2. 检查磁盘缓存或实时解析（后台线程）
            return await Task.Run(() =>
            {
                var cacheFilePath =
                    Path.Combine(_cacheFolderPath, $"{Path.GetFileNameWithoutExtension(fileName)}.json");

                // 检查磁盘缓存
                if (File.Exists(cacheFilePath))
                {
                    try
                    {
                        using (var stream = new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read,
                                   FileShare.Read, 4096))
                        using (var reader = new StreamReader(stream))
                        {
                            var json = reader.ReadToEnd();
                            videoFile = JsonConvert.DeserializeObject<VideoFile>(json);

                            var filePath = Path.Combine(_videoFolderPath, fileName);
                            if (videoFile != null && File.Exists(filePath) &&
                                File.GetLastWriteTime(filePath) == videoFile.LastModified)
                            {
                                // 更新内存缓存（线程安全）
                                _memoryCache[fileName] = videoFile;
                                return videoFile;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"读取缓存失败: {ex.Message}");
                    }
                }

                // 3. 实时解析（最耗时）
                var fullFilePath = Path.Combine(_videoFolderPath, fileName);
                if (File.Exists(fullFilePath))
                {
                    try
                    {
                        videoFile = ParseVideoMetadata(fullFilePath);
                        if (videoFile != null)
                        {
                            // 更新内存缓存（线程安全）
                            _memoryCache[fileName] = videoFile;
                            SaveToCacheAsync(videoFile);
                            return videoFile;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"解析视频失败: {ex.Message}");
                    }
                }

                return null;
            });
        }

        // 使用FFprobe解析视频元数据
        private VideoFile ParseVideoMetadata(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                // 并行执行获取时长和分辨率的任务
                var durationTask = Task.Run(() => GetVideoDuration(filePath));
                var resolutionTask = Task.Run(() => GetVideoResolution(filePath));

                Task.WaitAll(durationTask, resolutionTask);

                var coverImagePath = GenerateCoverImage(filePath);

                return new VideoFile
                {
                    VideoName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    VideoSize = $"{fileInfo.Length / 1024.0 / 1024.0:N2} MB",
                    Duration = durationTask.Result,
                    Resolution = resolutionTask.Result,
                    CoverImage = coverImagePath,
                    LastModified = fileInfo.LastWriteTime
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析视频元数据失败: {ex.Message}");
                return null;
            }
        }

        // 获取视频时长
        private string GetVideoDuration(string filePath)
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
                Debug.WriteLine($"获取视频时长失败: {ex.Message}");
            }

            return "未知";
        }

        // 获取视频分辨率
        private string GetVideoResolution(string filePath)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffprobe,
                        Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 \"{filePath}\"",
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
                Debug.WriteLine($"获取视频分辨率失败: {ex.Message}");
            }
            return "未知";
        }

        // 生成视频封面图
        private string GenerateCoverImage(string filePath)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var coverPath = Path.Combine(_cacheFolderPath, $"{fileName}.jpg");

                if (File.Exists(coverPath))
                    return coverPath;

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

        // 异步保存到缓存
        private async void SaveToCacheAsync(VideoFile videoFile)
        {
            try
            {
                var json = JsonConvert.SerializeObject(videoFile);
                var cacheFilePath = Path.Combine(_cacheFolderPath, $"{Path.GetFileNameWithoutExtension(videoFile.VideoName)}.json");
            
                using (var stream = new FileStream(cacheFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存缓存失败: {ex.Message}");
            }
        }
    }
}