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
                  '关于',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
              ),

              Divider(
                height: 20,
                indent: 15,
                endIndent: 15,
                color: Colors.grey[100],
              ),

              Expanded(
                child: Column(
                  children: [
                    Image.asset('images/application.png'),
                    _renderGradientText('M3U8资源下载器', 36),
                    _renderGradientText('软件版本：v1.0.0.0', 20),
                  ],
                ),
              ),

              Container(
                padding: EdgeInsets.all(10),
                child: Text(
                  'Copyright © CoderPeng 2025 All rights reserved.',
                  style: TextStyle(fontSize: 16),
                ),
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
            child: Column(
              children: [
                Expanded(
                  child: Column(
                    children: [
                      Image.asset('images/application.png'),
                      _renderGradientText('M3U8资源下载器', 30),
                      _renderGradientText('软件版本：v1.0.0.0', 18),
                    ],
                  ),
                ),

                Container(
                  padding: EdgeInsets.all(5),
                  child: Text(
                    'Copyright © CoderPeng 2025 All rights reserved.',
                    style: TextStyle(fontSize: 13),
                  ),
                ),
              ],
            ),
          ),
        ),
      );
    }
  }

  Widget _renderGradientText(String text, double fontSize) {
    return ShaderMask(
      shaderCallback: (Rect rect) {
        return LinearGradient(
          colors: [Colors.blue, Colors.purple],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ).createShader(rect);
      },
      blendMode: BlendMode.srcIn,
      child: Text(
        text,
        style: TextStyle(fontSize: fontSize, fontWeight: FontWeight.bold),
      ),
    );
  }
}
