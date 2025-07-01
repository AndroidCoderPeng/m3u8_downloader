import 'package:logger/logger.dart';

class Fogger {
  static var logger = Logger(
    printer: PrettyPrinter(), // 美观的打印格式
  );

  static void d(dynamic message) {
    logger.d(message);
  }
}
