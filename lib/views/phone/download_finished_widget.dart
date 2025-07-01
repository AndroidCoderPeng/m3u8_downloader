import 'package:flutter/material.dart';
import 'package:m3u8_downloader/models/video_file.dart';
import 'package:m3u8_downloader/utils/fogger.dart';
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
      // String? selectedFolderPath = prefs.getString("selected_folder_path");
      // if (selectedFolderPath == null) return;
      // List<String> result =
      //     Directory(selectedFolderPath)
      //         .listSync()
      //         .where((element) => element.path.endsWith(".mp4"))
      //         .map((e) => e.path)
      //         .toList();

      // List<VideoFile> videos = await VideoManager.getVideoFilesAsync(result);

      // setState(() {
      //   downloadFiles = videos;
      // });
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('已下载', style: TextStyle(fontSize: 18))),
      body: Container(
        color: Colors.grey[100],
        child: Container(
          width: double.infinity,
          height: double.infinity,
          margin: EdgeInsets.all(10),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(5),
          ),
          child: ListView.builder(
            shrinkWrap: true,
            physics: NeverScrollableScrollPhysics(),
            itemCount: downloadFiles.length,
            itemBuilder: (context, index) {
              return Material(
                child: InkWell(
                  onTap: () {
                    Fogger.d('点击了第 $index 项');
                  },
                  child: DownloadFinishedItemWidget(file: downloadFiles[index]),
                ),
              );
            },
          ),
        ),
      ),
    );
  }
}
