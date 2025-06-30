import 'package:flutter/material.dart';

class SoftwareAboutWidget extends StatefulWidget {
  const SoftwareAboutWidget({super.key});

  @override
  State<SoftwareAboutWidget> createState() => _SoftwareAboutWidgetState();
}

class _SoftwareAboutWidgetState extends State<SoftwareAboutWidget> {
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
              '关于',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ],
        ),
      ),
    );
  }
}
