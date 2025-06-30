import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:m3u8_downloader/utils/fogger.dart';

class SoftwareSettingWidget extends StatefulWidget {
  const SoftwareSettingWidget({super.key});

  @override
  State<SoftwareSettingWidget> createState() => _SoftwareSettingWidgetState();
}

class _SoftwareSettingWidgetState extends State<SoftwareSettingWidget> {
  void selectOutputDirectory() async {
    // 选择输出目录
    try {
      String? directoryPath = await FilePicker.platform.getDirectoryPath(
        dialogTitle: '选择保存目录',
        lockParentWindow: true,
      );

      if (directoryPath != null) {
        Fogger.d('选择的目录: $directoryPath');
      }
    } catch (e) {
      Fogger.d('选择目录时出错: $e');
    }
  }

  String? _selectedFileTypeValue = 'mp4';

  List<DropdownMenuItem<String>>? getFileTypeDropdowntems() {
    return ['ts', 'mp4']
        .map(
          (String value) => DropdownMenuItem(value: value, child: Text(value)),
        )
        .toList();
  }

  String? _selectedRetryTimesValue = '5';

  List<DropdownMenuItem<String>>? getRetryTimesDropdownItems() {
    return ['3', '4', '5', '6', '7', '8', '9', '10']
        .map(
          (String value) => DropdownMenuItem(value: value, child: Text(value)),
        )
        .toList();
  }

  bool _isSwitchOn = false;

  void onSwitchChanged(BuildContext context, bool value) {
    setState(() {
      _isSwitchOn = value;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.grey[100],
      //纵向布局，不是常规理解的列，其实是行
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
                '软件设置',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
            ),

            Divider(height: 20, color: Colors.grey[100]),

            ListTile(
              iconColor: Colors.amberAccent,
              leading: Icon(Icons.folder),
              title: Text('保存目录', style: TextStyle(fontSize: 16)),
              trailing: ElevatedButton(
                onPressed: selectOutputDirectory,
                style: ElevatedButton.styleFrom(
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(5), // 圆角半径
                    side: BorderSide(
                      color: Colors.grey[100]!,
                      width: 1,
                      style: BorderStyle.solid,
                    ),
                  ),
                ),
                child: Text('选择'),
              ),
            ),

            Divider(height: 20, color: Colors.grey[100]),

            ListTile(
              iconColor: Colors.blue,
              leading: Icon(Icons.movie),
              title: Text('保存格式', style: TextStyle(fontSize: 16)),
              trailing: DropdownButton(
                alignment: Alignment.center,
                value: _selectedFileTypeValue,
                hint: const Text('请选择'),
                onChanged: (String? newValue) {
                  setState(() {
                    _selectedFileTypeValue = newValue;
                  });
                },
                items: getFileTypeDropdowntems(),
              ),
            ),

            Divider(height: 20, color: Colors.grey[100]),

            ListTile(
              iconColor: Colors.green,
              leading: Icon(Icons.recycling),
              title: Text('下载重试次数', style: TextStyle(fontSize: 16)),
              trailing: DropdownButton(
                alignment: Alignment.center,
                value: _selectedRetryTimesValue,
                hint: const Text('请选择'),
                onChanged: (String? newValue) {
                  setState(() {
                    _selectedRetryTimesValue = newValue;
                    Fogger.d('重试次数：$_selectedRetryTimesValue');
                  });
                },
                items: getRetryTimesDropdownItems(),
              ),
            ),

            Divider(height: 20, color: Colors.grey[100]),

            ListTile(
              iconColor: Colors.red,
              leading: Icon(Icons.change_circle),
              title: Text('自动转码', style: TextStyle(fontSize: 16)),
              trailing: Transform.scale(
                scale: 0.8,
                child: Switch(
                  value: _isSwitchOn,
                  trackOutlineColor: WidgetStatePropertyAll(Colors.transparent),
                  activeColor: Colors.green,
                  inactiveTrackColor: Colors.grey[300],
                  thumbColor: WidgetStatePropertyAll(Colors.white),
                  onChanged: (value) => onSwitchChanged(context, value),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
