import 'dart:io';

import 'package:flutter/material.dart';

class SoftwareAboutWidget extends StatefulWidget {
  const SoftwareAboutWidget({super.key});

  @override
  State<SoftwareAboutWidget> createState() => _SoftwareAboutWidgetState();
}

class _SoftwareAboutWidgetState extends State<SoftwareAboutWidget> {
  @override
  Widget build(BuildContext context) {
    if (Platform.isWindows || Platform.isLinux || Platform.isMacOS) {
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
              Expanded(
                child: Row(
                  children: [
                    Expanded(
                      child: Column(
                        children: [
                          Image.asset('images/application.png'),
                          Text(
                            'M3U8资源下载器',
                            style: TextStyle(
                              fontSize: 36,
                              fontWeight: FontWeight.bold,
                              color: LinearGradient(colors: []),
                            ),
                          ),
                          Text(
                            '软件版本：v1.0.0.0',
                            style: TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
              Text(
                'Copyright © CoderPeng 2025 All rights reserved.',
                style: TextStyle(fontSize: 16),
              ),
            ],
          ),
        ),
      );
    } else {
      return Scaffold(
        appBar: AppBar(title: Text('关于', style: TextStyle(fontSize: 18))),
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
}
