//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

// Conditional flag initialization
/* Expected output:
A::A()
A::A(9)
A::A(const A&), i = 109
Top of loop, j = 0
A::A(1)
A::~A, i = 1
Bottom of loop, j = 0
Top of loop, j = 1
A::A(3)
A::~A, i = 3
Bottom of loop, j = 1
A::A(5)
A::A(11)
A::operator int, i = 11
A::~A, i = 11
A::A(7)
A::A(const A&), i = 107
A::~A, i = 7
A::~A, i = 5
A::~A, i = 107
A::~A, i = 109
A::~A, i = 9
A::~A, i = 5
*/
extern "C" int printf(const char *, ...);
struct A {
  int i;
  A(int n) : i(n) { printf("A::A(%d)\n", n); }
  A() : i(0) { printf("A::A()\n"); }
  A(const A& p) { i = 100+p.i; printf("A::A(const A&), i = %d\n", i); }
  ~A() { printf("A::~A, i = %d\n", i); }
  operator int() { printf("A::operator int, i = %d\n", i); return 0; }
} a;
int i = 1;
main () {
  for (int j = 0; j < 2; j++) {
    printf("Top of loop, j = %d\n", j);
    if (j == 0) goto lab;
    switch (j) {
      case 999:
        i = 2;
lab:  
        a = i ? A(1) : A(2);
        break;
      case 1:
        a = i ? A(3) : A(4);
        break;
    }  /* switch */
    printf("Bottom of loop, j = %d\n", j);
  }  /* for */
  a = i ? A(5) : A(6);
  for (; i ? A(11) : A(12); ) {}
  static A aa = i ? A(7) : A(8);
  return 0;
}
static A aaa = i ? A(9) : A(10);

