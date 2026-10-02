//options_all:-r -x -tused
//options: --strict;rp

#ifdef __cplusplus
extern "C" int printf(char*, ...);
#else
extern int printf();
#endif /* __cplusplus */

struct S1 {
  int a;
  char :0;
  char b;
  char :0;
  unsigned int c:1;
  char :0;
  char d;
};
struct S2 {
  int a;
  short :0;
  char b;
  short :0;
  unsigned int c:1;
  short :0;
  char d;
};
struct S3 {
  int a;
  int :0;
  char b;
  int :0;
  unsigned int c:1;
  int :0;
  char d;
};
static unsigned int i1[5] = { 0,0,0,0,0 };
static unsigned int i2[5] = { 0,0,0,0,0 };
static unsigned int i3[5] = { 0,0,0,0,0 };

int main() {
  struct S1 *ps1 = (struct S1 *)i1;
  struct S2 *ps2 = (struct S2 *)i2;
  struct S3 *ps3 = (struct S3 *)i3;

  ps1->a = ps1->b = ps1->c = ps1->d = 1;
  printf("%08x %08x %08x %08x %08x\n", i1[0], i1[1], i1[2], i1[3], i1[4]);
  ps2->a = ps2->b = ps2->c = ps2->d = 1;
  printf("%08x %08x %08x %08x %08x\n", i2[0], i2[1], i2[2], i2[3], i2[4]);
  ps3->a = ps3->b = ps3->c = ps3->d = 1;
  printf("%08x %08x %08x %08x %08x\n", i3[0], i3[1], i3[2], i3[3], i3[4]);
}

