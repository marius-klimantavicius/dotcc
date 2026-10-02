//options_all:-r -x -tused
//options: --strict;cn:;rp

extern "C" int printf(char *, ...);
struct S {
//  char c;
  int flags:10;
} s;
main() {
  printf("%d\n", sizeof(s));
}


