import 'package:flutter/material.dart';

class DividerWidget extends StatelessWidget {
  const DividerWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Divider(
      height: 20,
      indent: 15,
      endIndent: 15,
      color: Colors.grey[100],
    );
  }
}
