import 'dart:io';

import 'package:flutter/material.dart';
import 'package:m3u8_downloader/utils/desktop_video_manager.dart';
import 'package:m3u8_downloader/views/computer_platform_widget.dart';
import 'package:m3u8_downloader/views/mobile_platform_widget.dart';
import 'package:window_manager/window_manager.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // 初始化窗口管理（仅桌面平台需要）
  if (Platform.isWindows || Platform.isLinux || Platform.isMacOS) {
    await _initDesktopWindow();
  }

  runApp(const CrossPlatformApp());
}

// 桌面窗口初始化方法
Future<void> _initDesktopWindow() async {
  await windowManager.ensureInitialized();

  WindowOptions windowOptions = WindowOptions(
    title: "M3U8资源下载器",
    size: const Size(800, 600),
    center: true,
    skipTaskbar: false,
    alwaysOnTop: true,
  );

  await windowManager.waitUntilReadyToShow(windowOptions, () async {
    await windowManager.show();
    await windowManager.focus();
  });
}

// 跨平台应用包装器
class CrossPlatformApp extends StatelessWidget {
  const CrossPlatformApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(title: 'M3U8资源下载器', home: _getPlatformWidget());
  }

  // 根据平台返回对应组件
  Widget _getPlatformWidget() {
    if (Platform.isWindows || Platform.isLinux || Platform.isMacOS) {
      DesktopVideoManager.initialize();
      return const ComputerPlatformWidget();
    } else {
      return const MobilePlatformWidget();
    }
  }
}
