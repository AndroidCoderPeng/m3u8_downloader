import 'dart:io';

import 'package:flutter/material.dart';
import 'package:m3u8_downloader/models/video_file.dart';
import 'package:m3u8_downloader/utils/desktop_video_manager.dart';

class DownloadFinishedItemWidget extends StatelessWidget {
  const DownloadFinishedItemWidget({super.key, required this.file});

  final VideoFile file;

  Widget _renderCoverImage(String? path) {
    if (path != null && path.isNotEmpty) {
      return Image.file(
        File(path),
        width: 120,
        height: 85,
        fit: BoxFit.cover, // 控制图片填充方式
        errorBuilder: (context, error, stackTrace) {
          return const Icon(Icons.error);
        },
      );
    } else {
      return Image.asset('images/application.png');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.only(left: 15, right: 15, top: 5, bottom: 5),
      child: Row(
        children: [
          Stack(
            children: [
              ClipRRect(
                borderRadius: BorderRadius.circular(5),
                child: _renderCoverImage(file.coverImage),
              ),

              Positioned(
                top: 0,
                left: 3,
                child: Text(
                  file.resolution,
                  style: TextStyle(
                    fontSize: 12,
                    color: Colors.white,
                    shadows: [
                      Shadow(
                        blurRadius: 2.0,
                        color: Colors.black.withValues(alpha: 0.5),
                        offset: const Offset(1, 1),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),

          Expanded(
            child: Container(
              padding: EdgeInsets.only(left: 10),
              height: 85,
              child: Stack(
                children: [
                  Positioned(
                    top: 0,
                    left: 0,
                    right: 0,
                    child: Text(
                      file.videoName,
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.bold,
                      ),
                      overflow: TextOverflow.ellipsis,
                      maxLines: 2,
                      softWrap: true,
                    ),
                  ),

                  Positioned(
                    left: 0,
                    right: 0,
                    bottom: 0,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(file.videoSize, style: TextStyle(fontSize: 12)),
                        Text(file.duration, style: TextStyle(fontSize: 12)),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),

          IconButton(
            color: Colors.blue,
            icon: Icon(Icons.video_file_outlined),
            onPressed: () => {DesktopVideoManager.openFileWithDefault(file)},
          ),
        ],
      ),
    );
  }
}
