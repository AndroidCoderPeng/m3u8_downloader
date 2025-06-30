import 'package:flutter/material.dart';
import 'package:m3u8_downloader/views/computer_platform_widget.dart';
import 'package:m3u8_downloader/views/mobile_platform_widget.dart';
import 'package:window_manager/window_manager.dart';
import 'dart:io';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // 初始化窗口管理（仅桌面平台需要）
  if (Platform.isWindows || Platform.isLinux || Platform.isMacOS) {
    await initDesktopWindow();
  }

  runApp(const CrossPlatformApp());
}

// 桌面窗口初始化方法
Future<void> initDesktopWindow() async {
  await windowManager.ensureInitialized();

  WindowOptions windowOptions = WindowOptions(
    title: "M3U8资源下载器",
    size: const Size(800, 600),
    center: true,
    skipTaskbar: false,
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
    return MaterialApp(title: 'M3U8资源下载器', home: getPlatformWidget());
  }

  // 根据平台返回对应组件
  Widget getPlatformWidget() {
    if (Platform.isWindows || Platform.isLinux || Platform.isMacOS) {
      return const ComputerPlatformWidget();
    } else {
      return const MobilePlatformWidget();
    }
  }
}
