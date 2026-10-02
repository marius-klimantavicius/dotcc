//options_all:-r -x -tused
//options: --strict;cn:;rp

#define INT_MAX (0x7fffffff)
#define INT_MIN (0x80000000)
#define UINT_MAX (0xffffffff)
#define CHAR_BIT 8
//#include <limits.h>

enum t1 { A1 = -1, A2 = 1 };
enum t2 { B1 = INT_MIN, B2 = INT_MAX };
enum t3 { C1, C2 = UINT_MAX };
struct X {
  t1 e1 : 4;
  t2 e2 : sizeof(int) / sizeof(char) * CHAR_BIT;
  t3 e3 : sizeof(unsigned) / sizeof(char) * CHAR_BIT;
};
X x  = { A1, B1, C1 };

extern "C" int printf(char *, ...);
main() {
  printf("A1 = %x\n", A1);
  printf("sizeof(t1) = %d, bits = %d\n", sizeof(t1), sizeof(t1) * CHAR_BIT);
  printf("INT_MIN = %x\n", INT_MIN);
  printf("INT_MAX = %x\n", INT_MAX);
  printf("sizeof(t2) = %d, bits = %d\n", sizeof(t2), sizeof(t2) * CHAR_BIT);
  printf("UINT_MAX = %x\n", UINT_MAX);
  printf("sizeof(t3) = %d, bits = %d\n", sizeof(t3), sizeof(t3) * CHAR_BIT);
}

