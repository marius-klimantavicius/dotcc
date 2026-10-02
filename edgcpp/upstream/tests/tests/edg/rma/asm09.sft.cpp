//options_all:-r -x -tused
//options: --strict;cn:;cn

// -tused --microsoft
template <class T> void f(T) {
  asm { xxx yyy zzz }
  int i;
  asm xxx yyy zzz
  int j;
  asm ("xxx yyy zzz");
}
main() {
  f(0);
}

