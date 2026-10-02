//options_all:-r -x -tused
//options: --strict;cn:;cn

extern "C" {
  static void f(int) { }
  static void f(float) { }
}
main() {
  f(0);
  f(0.0);
}

