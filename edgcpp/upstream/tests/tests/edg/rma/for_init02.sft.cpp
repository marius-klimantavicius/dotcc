//options_all:-r -x -tused
//options: --cfront_3.0;rp:;rp

extern "C" int printf(char *, ...);
int f(int j) {
  int i = 0;
  {
    for (int i = 0; i < j; ++i) { }
    return i;                 // In cfront mode, f returns the value of j,
  }                           //   but the standard says it returns 0.
}
main() {
  printf("%d\n", f(11));
}

