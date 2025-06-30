import 'package:flutter/material.dart';
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

  List<DropdownMenuItem<String>>? getFileTypeDropdowntems() {
    return ['ts', 'mp4']
        .map(
          (String value) => DropdownMenuItem(value: value, child: Text(value)),
        )
        .toList();
  }

  List<DropdownMenuItem<String>>? getRetryTimesDropdownItems() {
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
        _saveFileType = prefs.getString("save_file_type");
        _retryTimesValue = prefs.getString("retry_times_value");
      });
    });
  }

  bool _isSwitchOn = false;

  void onSwitchChanged(bool value) {
    setState(() {
      _isSwitchOn = value;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('软件设置', style: TextStyle(fontSize: 18))),
      body: Container(
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
                    _isSwitchEnabled = _saveFileType == 'mp4';
                    setState(() {
                      _saveFileType = newValue;
                    });
                  },
                  items: getFileTypeDropdowntems(),
                ),
              ),

              createDivider(),

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
                  items: getRetryTimesDropdownItems(),
                ),
              ),

              createDivider(),

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
                            ? (value) => onSwitchChanged(value)
                            : null,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget createDivider() {
    return Divider(
      height: 20,
      indent: 15,
      endIndent: 15,
      color: Colors.grey[100],
    );
  }
}
