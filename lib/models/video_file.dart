class VideoFile {
  /// 封面
  String coverImage;

  /// 视频名
  String videoName;

  /// 文件路径
  String filePath;

  /// 分辨率
  String resolution;

  /// 视频大小
  String videoSize;

  /// 时长
  String duration;

  /// 文件修改时间，用于缓存有效性检查
  DateTime lastModified;

  // 构造函数
  VideoFile({
    required this.coverImage,
    required this.videoName,
    required this.filePath,
    required this.resolution,
    required this.videoSize,
    required this.duration,
    required this.lastModified,
  });

  // 从JSON反序列化
  factory VideoFile.fromJson(Map<String, dynamic> json) {
    return VideoFile(
      coverImage: json['coverImage'] ?? '',
      videoName: json['videoName'] ?? '',
      filePath: json['filePath'] ?? '',
      resolution: json['resolution'] ?? '',
      videoSize: json['videoSize'] ?? '',
      duration: json['duration'] ?? '',
      lastModified:
          DateTime.tryParse(json['lastModified'] ?? '') ?? DateTime.now(),
    );
  }

  // 序列化为JSON
  Map<String, dynamic> toJson() {
    return {
      'coverImage': coverImage,
      'videoName': videoName,
      'filePath': filePath,
      'resolution': resolution,
      'videoSize': videoSize,
      'duration': duration,
      'lastModified': lastModified.toIso8601String(),
    };
  }
}
