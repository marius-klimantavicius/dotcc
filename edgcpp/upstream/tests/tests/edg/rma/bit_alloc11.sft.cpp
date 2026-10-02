//options_all:-r -x -tused
//options: --strict;cn:;rp

extern "C" void printf(char *, ...);
struct S {
  char c;
  char :0;
  char d;
  short :9;
  char e;
  char :0;
} s;
main() {
  printf("offset of d = %d\n", (int)&s.d - (int)&s);
  printf("offset of e = %d\n", (int)&s.e - (int)&s);
  printf("sizeof(s) = %d\n", sizeof(s));
}

