//options_all:-r -x -tused
//options: --strict;cn:;cn

template <class T> void f(T) {
  asm xxx
}
main() {
  f(0);
}

