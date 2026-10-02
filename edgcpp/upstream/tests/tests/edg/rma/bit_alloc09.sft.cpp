//options_all:-r -x -tused
//options: --strict;cn:;rp

#ifdef __cplusplus
extern "C" int printf(char*, ...);
#else
extern int printf();
#endif /* __cplusplus */

struct A {
  char i:1;
  char c;
} x;
struct B {
  int i:1;
  char c;
} y;

main() {
  printf("sizeof(A) = %d\n", sizeof(struct A));
  printf("sizeof(B) = %d\n", sizeof(struct B));
}



