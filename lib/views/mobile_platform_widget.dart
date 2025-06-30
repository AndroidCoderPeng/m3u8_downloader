import 'package:flutter/material.dart';
import 'package:m3u8_downloader/views/phone/download_finished_widget.dart';
import 'package:m3u8_downloader/views/phone/download_segment_widget.dart';
import 'package:m3u8_downloader/views/phone/software_setting_widget.dart';
import 'package:m3u8_downloader/views/software_about_widget.dart';

class MobilePlatformWidget extends StatelessWidget {
  const MobilePlatformWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      theme: ThemeData(
        primarySwatch: Colors.blue, // 设置主色调
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

  static const List<Widget> _pages = [
    DownloadSegmentWidget(),
    DownloadFinishedWidget(),
    SoftwareSettingWidget(),
    SoftwareAboutWidget(),
  ];

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      body: Center(child: _pages[_selectedIndex]),
      bottomNavigationBar: BottomNavigationBar(
        items: const <BottomNavigationBarItem>[
          BottomNavigationBarItem(icon: Icon(Icons.download), label: '下载中'),
          BottomNavigationBarItem(
            icon: Icon(Icons.download_done),
            label: '已下载',
          ),
          BottomNavigationBarItem(icon: Icon(Icons.settings), label: '设置'),
          BottomNavigationBarItem(icon: Icon(Icons.info), label: '关于'),
        ],
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
        type: BottomNavigationBarType.fixed,
        backgroundColor: Theme.of(context).colorScheme.surface,
        selectedLabelStyle: TextStyle(
          color: Theme.of(context).colorScheme.primary,
        ),
        unselectedLabelStyle: TextStyle(
          color: Theme.of(context).colorScheme.onSurfaceVariant,
        ),
      ),
    );
  }
}
