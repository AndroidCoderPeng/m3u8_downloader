class FileUtil {
  // 格式化文件大小
  static String formatFileSize(int bytes) {
    if (bytes < 1024) {
      return '${bytes.toStringAsFixed(2)} B';
    }
    if (bytes < 1024 * 1024) {
      return '${(bytes / 1024).toStringAsFixed(2)} KB';
    }
    if (bytes < 1024 * 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
    }
    return '${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(2)} GB';
  }

  // 生成安全的文件名
  static String getSafeFileName(String filePath) {
    return filePath.replaceAll(RegExp(r'[\\/:*?"<>|]'), '_');
  }
}
