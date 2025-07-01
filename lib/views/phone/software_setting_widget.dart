import 'dart:io';

import 'package:flutter/material.dart';
import 'package:m3u8_downloader/utils/file_util.dart';
import 'package:m3u8_downloader/utils/fogger.dart';
import 'package:m3u8_downloader/views/divider_widget.dart';
import 'package:path/path.dart' as path;
import 'package:path_provider/path_provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SoftwareSettingWidget extends StatefulWidget {
  const SoftwareSettingWidget({super.key});

  @override
  State<SoftwareSettingWidget> createState() => _SoftwareSettingWidgetState();
}

class _SoftwareSettingWidgetState extends State<SoftwareSettingWidget> {
  late SharedPreferences prefs;
  String? _saveFileType;
  bool _isSwitchEnabled = true;
  String? _retryTimesValue;
  bool _isSwitchOn = true;
  String _cacheSize = '0.00 MB';

  List<DropdownMenuItem<String>>? _getFileTypeDropdownItems() {
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
      _saveFileType = prefs.getString("save_file_type");
      _retryTimesValue = prefs.getString("retry_times_value");
      String? autoEncodeValue = prefs.getString('auto_encode');
      if (autoEncodeValue == null || autoEncodeValue == '1') {
        setState(() {
          _isSwitchOn = true;
        });
      } else {
        setState(() {
          _isSwitchOn = false;
        });
      }

      // final directory = await getApplicationDocumentsDirectory();
      final cacheDir = Directory(path.join(directory.path, 'VideoCache'));
      if (!cacheDir.existsSync()) {
        setState(() {
          _cacheSize = '0.00 MB';
        });
      } else {
        int totalSize = 0;
        await for (var entity in cacheDir.list()) {
          if (entity is File) {
            var stat = await entity.stat();
            totalSize += stat.size;
          }
        }
        setState(() {
          _cacheSize = FileUtil.formatFileSize(totalSize);
        });
      }
    });
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

  void _clearCache() async {
    // final directory = await getApplicationDocumentsDirectory();
    final cacheDir = Directory(path.join(directory.path, 'VideoCache'));

    if (!cacheDir.existsSync()) {
      Fogger.d('缓存目录不存在');
      return;
    }

    if (!mounted) return;

    final bool? isConfirmed = await showDialog<bool>(
      context: context,
      builder:
          (context) => AlertDialog(
            title: const Text('提示'),
            content: const Text('确定清除缓存吗？'),
            actions: [
              TextButton(
                onPressed: Navigator.of(context).pop,
                child: const Text('取消'),
              ),
              TextButton(
                onPressed: () => Navigator.of(context).pop(true),
                child: const Text('确定'),
              ),
            ],
          ),
    );

    if (isConfirmed == true) {
      await cacheDir.delete(recursive: true);
      if (mounted) {
        setState(() {
          _cacheSize = '0.00 MB';
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('软件设置', style: TextStyle(fontSize: 18))),
      body: Container(
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
              Divider(height: 10, indent: 15, endIndent: 15),

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
                  items: _getFileTypeDropdownItems(),
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
                    trackOutlineColor: WidgetStatePropertyAll(
                      Colors.transparent,
                    ),
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

              DividerWidget(),

              ListTile(
                iconColor: Colors.amber,
                leading: Icon(Icons.delete),
                title: Container(
                  alignment: Alignment.topLeft,
                  child: SizedBox(
                    width: double.infinity,
                    child: Text('清除缓存', style: TextStyle(fontSize: 16)),
                  ),
                ),
                trailing: Text(_cacheSize, style: TextStyle(fontSize: 16)),
                onTap: _clearCache,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
