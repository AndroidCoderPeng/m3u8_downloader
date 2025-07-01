import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:m3u8_downloader/utils/fogger.dart';
import 'package:m3u8_downloader/views/divider_widget.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SoftwareSettingWidget extends StatefulWidget {
  const SoftwareSettingWidget({super.key});

  @override
  State<SoftwareSettingWidget> createState() => _SoftwareSettingWidgetState();
}

class _SoftwareSettingWidgetState extends State<SoftwareSettingWidget> {
  late SharedPreferences prefs;
  String? _selectedFolderPath;
  String? _saveFileType;
  bool _isSwitchEnabled = true;
  String? _retryTimesValue;
  bool _isSwitchOn = true;

  List<DropdownMenuItem<String>>? _getFileTypeDropdowntems() {
    return ['ts', 'mp4']
        .map(
          (String value) => DropdownMenuItem(value: value, child: Text(value)),
        )
        .toList();
  }

  List<DropdownMenuItem<String>>? _getRetryTimesDropdownItems() {
    return ['3', '4', '5', '6', '7', '8', '9', '10']
        .map(
          (String value) => DropdownMenuItem(value: value, child: Text(value)),
        )
        .toList();
  }

  @override
  void initState() {
    super.initState();
    Future.microtask(() async {
      prefs = await SharedPreferences.getInstance();
      setState(() {
        _selectedFolderPath = prefs.getString("selected_folder_path");
        _saveFileType = prefs.getString("save_file_type");
        _retryTimesValue = prefs.getString("retry_times_value");
        String? autoEncodeValue = prefs.getString('auto_encode');
        if (autoEncodeValue == null || autoEncodeValue == '1') {
          _isSwitchOn = true;
        } else {
          _isSwitchOn = false;
        }
      });
    });
  }

  void _selectOutputDirectory() async {
    // 选择输出目录
    try {
      String? directoryPath = await FilePicker.platform.getDirectoryPath(
        dialogTitle: '选择保存目录',
        lockParentWindow: true,
      );

      if (directoryPath == null) return;
      prefs.setString('selected_folder_path', directoryPath);
      setState(() {
        _selectedFolderPath = directoryPath;
      });
    } catch (e) {
      Fogger.d('选择目录时出错: $e');
    }
  }

  void _onSwitchChanged(bool value) {
    setState(() {
      if (value) {
        prefs.setString('auto_encode', '1');
      } else {
        prefs.setString('auto_encode', '0');
      }
      _isSwitchOn = value;
    });
  }

  @override
  Widget build(BuildContext context) {
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
                '软件设置',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
            ),

            DividerWidget(),

            ListTile(
              iconColor: Colors.amberAccent,
              leading: Icon(Icons.folder),
              title: Container(
                alignment: Alignment.topLeft,
                child: Column(
                  children: [
                    SizedBox(
                      width: double.infinity,
                      child: Text('保存目录', style: TextStyle(fontSize: 16)),
                    ),
                    SizedBox(
                      width: double.infinity,
                      child: Text(
                        _selectedFolderPath ?? '未选择',
                        style: TextStyle(fontSize: 12),
                      ),
                    ),
                  ],
                ),
              ),
              trailing: ElevatedButton(
                onPressed: _selectOutputDirectory,
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

            DividerWidget(),

            ListTile(
              iconColor: Colors.blue,
              leading: Icon(Icons.movie),
              title: Text('资源保存格式', style: TextStyle(fontSize: 16)),
              trailing: DropdownButton(
                alignment: Alignment.center,
                value: _saveFileType ?? 'mp4',
                hint: const Text('请选择'),
                underline: Container(height: 2, color: Colors.grey[300]),
                onChanged: (String? newValue) {
                  if (newValue == null) return;
                  prefs.setString('save_file_type', newValue);
                  if (newValue == 'mp4') {
                    _isSwitchEnabled = true;
                    setState(() {
                      _isSwitchOn = true;
                    });
                  } else {
                    _isSwitchEnabled = false;
                    setState(() {
                      _isSwitchOn = false;
                    });
                  }
                  setState(() {
                    _saveFileType = newValue;
                  });
                },
                items: _getFileTypeDropdowntems(),
              ),
            ),

            DividerWidget(),

            ListTile(
              iconColor: Colors.green,
              leading: Icon(Icons.recycling),
              title: Text('下载重试次数', style: TextStyle(fontSize: 16)),
              trailing: DropdownButton(
                alignment: Alignment.center,
                value: _retryTimesValue ?? '5',
                hint: const Text('请选择'),
                underline: Container(height: 2, color: Colors.grey[300]),
                onChanged: (String? newValue) {
                  if (newValue == null) return;
                  prefs.setString('retry_times_value', newValue);
                  setState(() {
                    _retryTimesValue = newValue;
                  });
                },
                items: _getRetryTimesDropdownItems(),
              ),
            ),

            DividerWidget(),

            ListTile(
              iconColor: Colors.purple,
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
                  onChanged:
                      _isSwitchEnabled
                          ? (value) => _onSwitchChanged(value)
                          : null,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
