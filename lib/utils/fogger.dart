import 'package:logger/logger.dart';

var logger = Logger(
  printer: PrettyPrinter(), // 美观的打印格式
);

class Fogger {
  static void d(dynamic message) {
    logger.d(message);
  }
}
