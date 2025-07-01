import 'dart:convert';
import 'dart:io';
import 'package:ffmpeg_kit_flutter_new/ffmpeg_kit_config.dart';
import 'package:ffmpeg_kit_flutter_new/level.dart';
import 'package:ffmpeg_kit_flutter_new/log.dart';
import 'package:ffmpeg_kit_flutter_new/return_code.dart';
import 'package:ffmpeg_kit_flutter_new/statistics.dart';
import 'package:path/path.dart' as path;
import 'package:ffmpeg_kit_flutter_new/ffmpeg_kit.dart';
import 'package:m3u8_downloader/models/video_file.dart';
import 'package:m3u8_downloader/utils/fogger.dart';
import 'package:path_provider/path_provider.dart';
import 'package:video_thumbnail/video_thumbnail.dart';

class VideoManager {
  // 内存缓存
  static final Map<String, VideoFile> _memoryCache = {};
  // 硬盘缓存目录
  static String? _cacheDirectory;

  // 初始化方法 - 新增
  static Future<void> initialize() async {
    try {
      // 初始化FFmpegKit
      await _initializeFFmpegKit();

      // 初始化缓存目录
      await _initCacheDirectory();
    } catch (e) {
      Fogger.d('VideoManager初始化失败: $e');
      rethrow;
    }
  }

  static Future<void> _initializeFFmpegKit() async {}

  static Future<void> _initCacheDirectory() async {
    try {
      final directory = await getApplicationDocumentsDirectory();

      // 设置缓存目录为工作目录下的cache文件夹
      _cacheDirectory = path.join(directory.path, 'VideoCache');
      final cacheDir = Directory(_cacheDirectory!);

      if (!await cacheDir.exists()) {
        await cacheDir.create(recursive: true);
      }
      Fogger.d('缓存目录已设置为: $_cacheDirectory');
    } catch (e) {
      Fogger.d('初始化缓存目录失败: $e');
      _cacheDirectory = path.join(Directory.systemTemp.path, 'VideoCache');
      await Directory(_cacheDirectory!).create(recursive: true);
    }
  }

  static Future<List<VideoFile>> getVideoFilesAsync(
    List<String> videoPaths,
  ) async {
    final futures =
        videoPaths.map((videoPath) async {
          try {
            return await _getVideoFileWithCache(videoPath);
          } catch (e) {
            Fogger.d('获取视频失败: $videoPath, 错误: $e');
            return null;
          }
        }).toList();
    final results = await Future.wait(futures);
    return results.whereType<VideoFile>().toList();
  }

  // 从多级缓存获取单个视频文件
  static Future<VideoFile?> _getVideoFileWithCache(String videoPath) async {
    // 1. 尝试从内存缓存获取
    if (_memoryCache.containsKey(videoPath)) {
      return _memoryCache[videoPath];
    }

    // 2. 尝试从硬盘缓存获取
    final cachedFile = await _loadFromDiskCache(videoPath);
    if (cachedFile != null) {
      _memoryCache[videoPath] = cachedFile;
      return cachedFile;
    }

    // 3. 实时获取（使用ffmpeg）
    final videoFile = await _fetchVideoFile(videoPath);
    if (videoFile != null) {
      _memoryCache[videoPath] = videoFile;
      await _saveToDiskCache(videoFile);
    }
    return videoFile;
  }

  // 从硬盘加载缓存
  static Future<VideoFile?> _loadFromDiskCache(String videoPath) async {
    if (_cacheDirectory == null) return null;

    final cacheFilePath = path.join(
      _cacheDirectory!,
      '${_getSafeFileName(videoPath)}.cache',
    );

    final file = File(cacheFilePath);
    if (!await file.exists()) return null;

    try {
      final jsonString = await file.readAsString();
      final jsonMap = json.decode(jsonString) as Map<String, dynamic>;

      final cachedFile = VideoFile.fromJson(jsonMap);
      final actualFile = File(videoPath);
      if (await actualFile.exists()) {
        final actualModified = await actualFile.lastModified();
        if (actualModified == cachedFile.lastModified) {
          return cachedFile;
        }
      }
    } catch (e) {
      Fogger.d('加载硬盘缓存失败: $e');
    }
    return null;
  }

  // 保存到硬盘缓存
  static Future<void> _saveToDiskCache(VideoFile videoFile) async {
    if (_cacheDirectory == null) return;

    try {
      final cacheFilePath = path.join(
        _cacheDirectory!,
        '${_getSafeFileName(videoFile.filePath)}.cache',
      );

      final jsonString = json.encode(videoFile.toJson());
      final file = File(cacheFilePath);
      await file.writeAsString(jsonString);
    } catch (e) {
      Fogger.d('保存硬盘缓存失败: $e');
    }
  }

  // 使用ffmpeg实时获取视频信息
  static Future<VideoFile?> _fetchVideoFile(String videoPath) async {
    try {
      final file = File(videoPath);
      if (!await file.exists()) return null;

      final stat = await file.stat();
      final extension = path.extension(videoPath).toLowerCase();

      if (!['.mp4'].contains(extension)) {
        return null;
      }

      String resolution = '未知';
      String duration = '未知';

      // 使用ffmpeg_kit_flutter_new获取元数据
      final session = await FFmpegKit.executeAsync('-i "$videoPath"');
      final returnCode = await session.getReturnCode();

      if (returnCode != null && ReturnCode.isSuccess(returnCode)) {
        final output = await session.getOutput();

        final resolutionRegExp = RegExp(r'(\d+)x(\d+)');
        final resolutionMatch = resolutionRegExp.firstMatch(output ?? '');
        if (resolutionMatch != null) {
          resolution =
              '${resolutionMatch.group(1)}x${resolutionMatch.group(2)}';
        }

        final durationRegExp = RegExp(r'Duration: (\d+:\d+:\d+)');
        final durationMatch = durationRegExp.firstMatch(output ?? '');
        if (durationMatch != null) {
          duration = durationMatch.group(1) ?? '未知';
        }
      } else {
        Fogger.d('FFmpeg执行失败: ${returnCode?.toString()}');
      }

      String? coverImagePath;
      try {
        final tempDir = await Directory.systemTemp.createTemp();
        coverImagePath = await VideoThumbnail.thumbnailFile(
          video: videoPath,
          thumbnailPath: tempDir.path,
          imageFormat: ImageFormat.PNG,
          maxHeight: 240,
          quality: 75,
        );
        await tempDir.delete(recursive: true);
      } catch (e) {
        Fogger.d('生成缩略图失败: $e');
      }

      return VideoFile(
        coverImage: coverImagePath ?? '',
        videoName: path.basenameWithoutExtension(videoPath),
        filePath: videoPath,
        resolution: resolution,
        videoSize: _formatFileSize(stat.size),
        duration: duration,
        lastModified: stat.modified,
      );
    } catch (e) {
      Fogger.d('获取视频信息失败: $e');
      return null;
    }
  }

  // 格式化文件大小
  static String _formatFileSize(int bytes) {
    if (bytes < 1024) {
      return '$bytes B';
    }
    if (bytes < 1024 * 1024) {
      return '${(bytes / 1024).toStringAsFixed(1)} KB';
    }
    if (bytes < 1024 * 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB';
    }
    return '${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(1)} GB';
  }

  // 生成安全的文件名
  static String _getSafeFileName(String filePath) {
    return filePath.replaceAll(RegExp(r'[\\/:*?"<>|]'), '_');
  }
}
