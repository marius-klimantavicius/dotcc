//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cp

extern "C" void printf(char*, ...);
struct S {
  virtual void f();
  int a, b, c[];
} s;
void S::f() { }
main() {
  printf("sizeof s is %d\n", sizeof(s));
  printf("offset of a is %d\n", (int)(&s.a) - (int)(&s));
  printf("offset of b is %d\n", (int)(&s.b) - (int)(&s));
  printf("offset of c is %d\n", (int)(&s.c) - (int)(&s));
}

