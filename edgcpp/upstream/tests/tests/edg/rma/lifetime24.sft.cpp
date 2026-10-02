//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cp

//type:rp
// Construction/destruction in expressions of "for" statement
extern "C" int printf(const char *, ...);
int i =4;
struct A {
  A() { printf("A::A()\n"); }
  ~A() { printf("A::~A()\n"); }
  operator int() { printf("A::operator int\n");  return i--; }
};
main () {
  for (A a; printf("test\n"), A(); printf("incr\n"), A()) printf("loop\n");
  printf("end loop\n");
  return 0;
}

