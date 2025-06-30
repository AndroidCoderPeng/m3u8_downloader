import 'package:flutter/material.dart';

class DownloadFinishedWidget extends StatefulWidget {
  const DownloadFinishedWidget({super.key});

  @override
  State<DownloadFinishedWidget> createState() => _DownloadFinishedWidgetState();
}

class _DownloadFinishedWidgetState extends State<DownloadFinishedWidget> {
  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.grey[100],
      //纵向布局，不是常规理解的列，其实是行
      child: Container(
        width: double.infinity,
        height: double.infinity,
        margin: EdgeInsets.all(10),
        padding: EdgeInsets.all(10),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(5),
        ),
        child: Column(
          children: [
            Text(
              '已下载',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ],
        ),
      ),
    );
  }
}
