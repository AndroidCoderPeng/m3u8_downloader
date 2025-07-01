import 'dart:io';
import 'package:flutter/material.dart';
import 'package:m3u8_downloader/views/divider_widget.dart';
import 'package:m3u8_downloader/views/download_finished_item_widget.dart';
import 'package:shared_preferences/shared_preferences.dart';

class DownloadFinishedWidget extends StatefulWidget {
  const DownloadFinishedWidget({super.key});

  @override
  State<DownloadFinishedWidget> createState() => _DownloadFinishedWidgetState();
}

class _DownloadFinishedWidgetState extends State<DownloadFinishedWidget> {
  late SharedPreferences prefs;
  List<String> downloadFiles = [];

  @override
  void initState() {
    super.initState();
    Future.microtask(() async {
      prefs = await SharedPreferences.getInstance();
      String? selectedFolderPath = prefs.getString("selected_folder_path");
      if (selectedFolderPath == null) return;
      List<String> result =
          Directory(selectedFolderPath)
              .listSync()
              .where((element) => element.path.endsWith(".mp4"))
              .map((e) => e.path)
              .toList();
      setState(() {
        downloadFiles = result;
      });
    });
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.grey[100],
      //纵向布局，不是常规理解的列，其实是行
      child: Container(
        width: double.infinity,
        height: double.infinity,
        margin: EdgeInsets.all(10),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(5),
        ),
        child: Column(
          children: [
            ListTile(
              title: Text(
                '已下载',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
            ),

            DividerWidget(),

            // 已下载列表
            ListView.builder(
              shrinkWrap: true,
              physics: NeverScrollableScrollPhysics(),
              itemCount: downloadFiles.length,
              itemBuilder: (context, index) {
                return DownloadFinishedItemWidget(
                  filePath: downloadFiles[index],
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}
