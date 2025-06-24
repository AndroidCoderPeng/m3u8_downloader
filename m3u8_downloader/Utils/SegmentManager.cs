using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using m3u8_downloader.Models;
using Newtonsoft.Json;

namespace m3u8_downloader.Utils
{
    public class SegmentManager
    {
        private readonly string _segmentFolderPath;
        private readonly string _cacheFolderPath;
        private readonly ConcurrentDictionary<string, SegmentFile> _memoryCache;

        public SegmentManager(string segmentFolderPath)
        {
            _segmentFolderPath = segmentFolderPath;
            _cacheFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            _memoryCache = new ConcurrentDictionary<string, SegmentFile>();
            
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
                            var segmentFile = JsonConvert.DeserializeObject<SegmentFile>(json);
                            if (segmentFile != null && File.Exists(segmentFile.FilePath))
                            {
                                _memoryCache[Path.GetFileName(segmentFile.FilePath)] = segmentFile;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"加载缓存文件失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"加载缓存文件夹失败: {ex.Message}");
            }
        }

        public async Task<List<SegmentFile>> GetSegmentsAsync()
        {
            try
            {
                var files = Directory.GetFiles(_segmentFolderPath, "*.ts");
                var tasks = files.Select(file => GetSegmentByFileNameAsync(Path.GetFileName(file)));
                var results = await Task.WhenAll(tasks);
                return results.Where(v => v != null).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"加载视频片段时发生错误: {ex.Message}");
                return new List<SegmentFile>();
            }
        }

        private async Task<SegmentFile> GetSegmentByFileNameAsync(string fileName)
        {
            if (_memoryCache.TryGetValue(fileName, out var segmentFile))
            {
                var filePath = Path.Combine(_segmentFolderPath, fileName);
                if (File.Exists(filePath) && File.GetLastWriteTime(filePath) == segmentFile.LastModified)
                {
                    return segmentFile;
                }
            }

            return await Task.Run(() =>
            {
                var cacheFilePath =
                    Path.Combine(_cacheFolderPath, $"{Path.GetFileNameWithoutExtension(fileName)}.json");
                if (File.Exists(cacheFilePath))
                {
                    try
                    {
                        using (var stream = new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read,
                                   FileShare.Read, 4096))
                        using (var reader = new StreamReader(stream))
                        {
                            var json = reader.ReadToEnd();
                            segmentFile = JsonConvert.DeserializeObject<SegmentFile>(json);

                            var filePath = Path.Combine(_segmentFolderPath, fileName);
                            if (segmentFile != null && File.Exists(filePath) &&
                                File.GetLastWriteTime(filePath) == segmentFile.LastModified)
                            {
                                // 更新内存缓存（线程安全）
                                _memoryCache[fileName] = segmentFile;
                                return segmentFile;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"读取缓存失败: {ex.Message}");
                    }
                }

                var fullFilePath = Path.Combine(_segmentFolderPath, fileName);
                if (File.Exists(fullFilePath))
                {
                    try
                    {
                        segmentFile = ParseSegmentMetadata(fullFilePath);
                        if (segmentFile != null)
                        {
                            // 更新内存缓存（线程安全）
                            _memoryCache[fileName] = segmentFile;
                            SaveToCacheAsync(segmentFile);
                            return segmentFile;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"解析视频片段失败: {ex.Message}");
                    }
                }

                return null;
            });
        }

        private SegmentFile ParseSegmentMetadata(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                // 并行执行获取时长和分辨率的任务
                var durationTask = Task.Run(filePath.GetMediaDuration);

                Task.WaitAll(durationTask);

                var coverImagePath = filePath.GenerateCoverImage(_cacheFolderPath);

                return new SegmentFile
                {
                    SegmentName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    SegmentSize = $"{fileInfo.Length / 1024.0 / 1024.0:N2} MB",
                    Duration = durationTask.Result,
                    CoverImage = coverImagePath,
                    LastModified = fileInfo.LastWriteTime
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"解析视频片段元数据失败: {ex.Message}");
                return null;
            }
        }

        private async void SaveToCacheAsync(SegmentFile segmentFile)
        {
            try
            {
                var json = JsonConvert.SerializeObject(segmentFile);
                var cacheFilePath = Path.Combine(_cacheFolderPath,
                    $"{Path.GetFileNameWithoutExtension(segmentFile.SegmentName)}.json");

                using (var stream = new FileStream(cacheFilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                           4096, true))
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"保存缓存失败: {ex.Message}");
            }
        }
    }
}