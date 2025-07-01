import 'package:flutter/material.dart';
import 'package:m3u8_downloader/views/desktop/download_finished_widget.dart';
import 'package:m3u8_downloader/views/desktop/download_segment_widget.dart';
import 'package:m3u8_downloader/views/desktop/software_setting_widget.dart';
import 'package:m3u8_downloader/views/software_about_widget.dart';

class ComputerPlatformWidget extends StatelessWidget {
  const ComputerPlatformWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      theme: ThemeData(
        primarySwatch: Colors.blue,
        // 设置主色调
        colorScheme: ColorScheme.fromSwatch(
          primarySwatch: Colors.blue, // 主色调
          accentColor: Colors.green, // 强调色 (Flutter 2.x 之前的 accentColor)
          brightness: Brightness.light, // 亮色主题
        ),
        appBarTheme: AppBarTheme(
          backgroundColor: Colors.blue, // 应用栏背景色
          foregroundColor: Colors.white, // 应用栏文本和图标颜色
        ),
        floatingActionButtonTheme: FloatingActionButtonThemeData(
          backgroundColor: Colors.blue, // 浮动按钮背景色
        ),
        useMaterial3: true, // 启用 Material Design 3
      ),
      home: const ApplicationHomePage(),
    );
  }
}

class ApplicationHomePage extends StatefulWidget {
  const ApplicationHomePage({super.key});

  @override
  State<ApplicationHomePage> createState() => _ApplicationHomePageState();
}

class _ApplicationHomePageState extends State<ApplicationHomePage> {
  int _selectedIndex = 0;

  // 定义导航项
  final List<NavigationRailDestination> _destinations = [
    const NavigationRailDestination(
      icon: Icon(Icons.download),
      label: Text('下载中'),
    ),
    const NavigationRailDestination(
      icon: Icon(Icons.download_done),
      label: Text('已完成'),
    ),
    const NavigationRailDestination(
      icon: Icon(Icons.settings),
      label: Text('设置'),
    ),
    const NavigationRailDestination(icon: Icon(Icons.info), label: Text('关于')),
  ];

  // 定义页面内容
  final List<Widget> _pages = [
    const DownloadSegmentWidget(),
    const DownloadFinishedWidget(),
    const SoftwareSettingWidget(),
    const SoftwareAboutWidget(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          NavigationRail(
            destinations: _destinations,
            selectedIndex: _selectedIndex,
            labelType: NavigationRailLabelType.all,
            backgroundColor: Colors.grey[100],
            selectedLabelTextStyle: TextStyle(
              color: Theme.of(context).colorScheme.secondary,
              fontFamily: "微软雅黑",
            ),
            unselectedLabelTextStyle: TextStyle(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
              fontFamily: "微软雅黑",
            ),
            onDestinationSelected: (int index) {
              setState(() {
                _selectedIndex = index;
              });
            },
          ),
          Expanded(child: _pages[_selectedIndex]),
        ],
      ),
    );
  }
}
