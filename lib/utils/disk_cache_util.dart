import 'dart:convert';
import 'dart:io';

import 'package:m3u8_downloader/models/video_file.dart';
import 'package:m3u8_downloader/utils/file_util.dart';
import 'package:m3u8_downloader/utils/fogger.dart';
import 'package:path/path.dart' as path;

class DiskCacheUtil {
  // 从硬盘加载缓存
  static Future<VideoFile?> loadFromDiskCache(
    String? cacheDirectory,
    String videoPath,
  ) async {
    if (cacheDirectory == null) return null;

    final cacheFilePath = path.join(
      cacheDirectory,
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
  static Future<void> saveToDiskCache(
    String? cacheDirectory,
    VideoFile videoFile,
  ) async {
    if (cacheDirectory == null) return;

    try {
      final cacheFilePath = path.join(
        cacheDirectory,
        '${FileUtil.getSafeFileName(videoFile.filePath)}.cache',
      );

      final jsonString = json.encode(videoFile.toJson());
      final file = File(cacheFilePath);
      await file.writeAsString(jsonString);
    } catch (e) {
      Fogger.d('保存硬盘缓存失败: $e');
    }
  }
}
