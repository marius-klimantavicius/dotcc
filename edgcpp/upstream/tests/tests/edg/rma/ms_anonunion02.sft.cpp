//options_all:-r --microsoft_version=1400
//type:rp

extern "C" int printf(char *, ...);
struct S {
  union { int a, b; };
  union { int b, c; };
} s;
main() {
  s.a = 1;
  s.c = 2;
  printf("%d\n", s.b);
}

