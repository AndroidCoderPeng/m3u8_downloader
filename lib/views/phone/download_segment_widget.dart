import 'package:flutter/material.dart';

class DownloadSegmentWidget extends StatefulWidget {
  const DownloadSegmentWidget({super.key});

  @override
  State<DownloadSegmentWidget> createState() => _DownloadSegmentWidgetState();
}

class _DownloadSegmentWidgetState extends State<DownloadSegmentWidget> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('下载中', style: TextStyle(fontSize: 18))),
      body: Container(
        color: Colors.grey[100],
        child: Container(
          width: double.infinity,
          height: double.infinity,
          margin: EdgeInsets.all(10),
          padding: EdgeInsets.all(10),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(5),
          ),
          child: Column(children: [
              
            ],
          ),
        ),
      ),
    );
  }
}
