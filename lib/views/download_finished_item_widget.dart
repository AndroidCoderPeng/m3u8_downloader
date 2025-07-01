import 'package:flutter/material.dart';

class DownloadFinishedItemWidget extends StatefulWidget {
  final String filePath;

  const DownloadFinishedItemWidget({super.key, required this.filePath});

  @override
  State<DownloadFinishedItemWidget> createState() =>
      _DownloadFinishedItemWidgetState();
}

class _DownloadFinishedItemWidgetState
    extends State<DownloadFinishedItemWidget> {
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.only(left: 15, right: 15, top: 5, bottom: 5),
      child: Row(
        children: [
          Stack(
            children: [
              Container(
                width: 120,
                height: 85,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(5),
                  color: Colors.grey[200],
                ),
                child: Image.asset('images/application.ico'),
              ),

              Positioned(
                top: 0,
                left: 3,
                child: Text(
                  '1080 x 1920',
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
                      widget.filePath,
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
                        Text('60.20 MB', style: TextStyle(fontSize: 12)),
                        Text('00:01:09', style: TextStyle(fontSize: 12)),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
