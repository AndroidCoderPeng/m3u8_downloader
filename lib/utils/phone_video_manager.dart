import 'dart:convert';
import 'dart:io';

import 'package:m3u8_downloader/models/video_file.dart';
import 'package:m3u8_downloader/utils/file_util.dart';
import 'package:m3u8_downloader/utils/fogger.dart';
import 'package:path/path.dart' as path;
import 'package:path_provider/path_provider.dart';

class PhoneVideoManager {
  // 内存缓存
  static final Map<String, VideoFile> _memoryCache = {};

  // 硬盘缓存目录
  static String? _cacheDirectory;

  // 初始化
  static Future<void> initialize() async {
    try {
      await _initializeFFmpeg();

      // 初始化缓存目录
      await _initCacheDirectory();
    } catch (e) {
      Fogger.d('初始化失败: $e');
      rethrow;
    }
  }

  static Future<void> _initializeFFmpeg() async {}

  static Future<void> _initCacheDirectory() async {
    try {
      final directory = await getApplicationDocumentsDirectory();

      // 设置缓存目录为工作目录下的cache文件夹
      _cacheDirectory = path.join(directory.path, 'VideoCache');
      final cacheDir = Directory(_cacheDirectory!);

      if (!await cacheDir.exists()) {
        await cacheDir.create(recursive: true);
      }
      /**
       * /data/user/0/com.pengxh.flutter.app.m3u8_downloader/app_flutter/VideoCache
       */
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
    VideoFile? videoFile = await _fetchVideoFileOnPhone(videoPath);
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
      '${FileUtil.getSafeFileName(videoPath)}.cache',
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
        '${FileUtil.getSafeFileName(videoFile.filePath)}.cache',
      );

      final jsonString = json.encode(videoFile.toJson());
      final file = File(cacheFilePath);
      await file.writeAsString(jsonString);
    } catch (e) {
      Fogger.d('保存硬盘缓存失败: $e');
    }
  }

  // 使用ffmpeg实时获取视频信息
  static Future<VideoFile?> _fetchVideoFileOnDesktop(String videoPath) async {
    try {
      final file = File(videoPath);
      if (!await file.exists()) return null;

      final stat = await file.stat();
      final extension = path.extension(videoPath).toLowerCase();

      if (!['.mp4'].contains(extension)) {
        return null;
      }

      String resolution = await _getVideoResolution(videoPath) ?? '未知';
      String duration = await _getMediaDuration(videoPath) ?? '未知';
      String imagePath = await _generateCoverImage(videoPath) ?? '';

      return VideoFile(
        coverImage: imagePath,
        videoName: path.basename(videoPath),
        filePath: videoPath,
        resolution: resolution,
        videoSize: FileUtil.formatFileSize(stat.size),
        duration: duration,
        lastModified: stat.modified,
      );
    } catch (e) {
      Fogger.d('获取视频信息失败: $e');
      return null;
    }
  }

  static Future<String?> _getVideoResolution(String videoPath) async {
    final result = await Process.run('ffprobe', [
      '-v',
      'error',
      '-select_streams',
      'v:0',
      '-show_entries',
      'stream=width,height',
      '-of',
      'csv=s=x:p=0',
      videoPath,
    ]);
    String? session = result.stdout;
    if (session == null) return null;
    final lines = session.split('\n');
    for (final line in lines) {
      if (line.isNotEmpty) {
        final resolution = line.split('x');
        return '${resolution[0]}x${resolution[1]}';
      }
    }
    return null;
  }

  static Future<String?> _getMediaDuration(String videoPath) async {
    final result = await Process.run('ffprobe', [
      '-v',
      'error',
      '-show_entries',
      'stream=duration',
      '-of',
      'default=noprint_wrappers=1:nokey=1',
      videoPath,
    ]);
    String? session = result.stdout;
    if (session == null) return null;
    final lines = session.split('\n');
    double? totalSeconds = 0;
    for (final line in lines) {
      totalSeconds = double.tryParse(line.trim());
      if (totalSeconds != null) {
        break;
      }
    }
    final durationObj = Duration(seconds: totalSeconds!.round());
    final hours = durationObj.inHours;
    final minutes = durationObj.inMinutes.remainder(60);
    final seconds = durationObj.inSeconds.remainder(60);
    if (hours > 0) {
      return '${hours.toString().padLeft(2, '0')}:'
          '${minutes.toString().padLeft(2, '0')}:'
          '${seconds.toString().padLeft(2, '0')}';
    } else {
      return '${minutes.toString().padLeft(2, '0')}:'
          '${seconds.toString().padLeft(2, '0')}';
    }
  }

  static Future<String?> _generateCoverImage(String videoPath) async {
    final fileName = path.basenameWithoutExtension(videoPath);
    final imagePath = path.join(_cacheDirectory!, '$fileName.jpg');
    await Process.run('ffmpeg', [
      '-i',
      videoPath,
      '-ss',
      '00:00:01.000',
      '-vframes',
      '1',
      imagePath,
    ]);
    return imagePath;
  }

  static Future<VideoFile?> _fetchVideoFileOnPhone(String videoPath) async {
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
      String coverImagePath = '未知';

      return VideoFile(
        coverImage: coverImagePath,
        videoName: path.basename(videoPath),
        filePath: videoPath,
        resolution: resolution,
        videoSize: FileUtil.formatFileSize(stat.size),
        duration: duration,
        lastModified: stat.modified,
      );
    } catch (e) {
      Fogger.d('获取视频信息失败: $e');
      return null;
    }
  }

  static Future<String?> _executeFFmpegCommand(List<String> arguments) async {
    try {} catch (e) {
      Fogger.d('执行FFmpeg命令失败: $e');
      return null;
    }
  }
}
