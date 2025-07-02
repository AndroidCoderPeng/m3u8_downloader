import 'dart:io';

import 'package:flutter/material.dart';
import 'package:m3u8_downloader/models/video_file.dart';
import 'package:m3u8_downloader/utils/desktop_video_manager.dart';
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
  List<VideoFile> downloadFiles = [];

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

      List<VideoFile> videos = await DesktopVideoManager.getVideoFilesAsync(
        result,
      );

      setState(() {
        downloadFiles = videos;
      });
    });
  }

  void _showContextMenu(BuildContext context, int index, Offset tapPosition) {
    showMenu(
      context: context,
      position: RelativeRect.fromLTRB(
        tapPosition.dx,
        tapPosition.dy,
        tapPosition.dx + 100,
        tapPosition.dy + 100,
      ),
      items: [
        PopupMenuItem(
          value: 'delete',
          child: Row(
            children: [
              Icon(Icons.delete, color: Colors.red),
              SizedBox(width: 8),
              Text('删除'),
            ],
          ),
        ),
      ],
    ).then((value) {
      if (value == null) return;
      // 删除文件
      File(downloadFiles[index].filePath).deleteSync();
      setState(() {
        downloadFiles.removeAt(index);
      });
    });
  }

  @override
  Widget build(BuildContext context) {
    if (downloadFiles.isEmpty) {
      return Container(
        color: Colors.grey[100],
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
              Expanded(child: Image.asset('images/empty_image.png')),
            ],
          ),
        ),
      );
    } else {
      return Container(
        color: Colors.grey[100],
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
                  return Material(
                    child: InkWell(
                      onSecondaryTapDown: (details) {
                        _showContextMenu(
                          context,
                          index,
                          details.globalPosition,
                        );
                      },
                      child: DownloadFinishedItemWidget(
                        file: downloadFiles[index],
                      ),
                    ),
                  );
                },
              ),
            ],
          ),
        ),
      );
    }
  }
}
