//options_all:-r -x -tused
//options: --strict;cn:;rp

extern "C" int printf(char *, ...);
int i = 13;
template <class T> int f(T t) {
  for (int i = 0; i < t; ++i) { }
  return i;
}
template <class T> int g(T t) {
  return i;
}
    
main() {
  for (int i = 0; i < 2; ++i) {
    printf("%d\n", g(i));
  }
  int i = 4;
  printf("%d\n", f(i));
}

