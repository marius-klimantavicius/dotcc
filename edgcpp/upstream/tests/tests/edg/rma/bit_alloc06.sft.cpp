//options_all:-r -x -tused
//options: --strict;rp

#ifdef __cplusplus
extern "C" int printf(char*, ...);
#else
extern int printf();
#endif /* __cplusplus */

struct S1 {
  char a;
  int :0;
  char b;
  int :0;
  char c:1;
  int :0;
  char d;
};
static unsigned char array[5*sizeof(int)] = { 0 };
//static unsigned int i1[5] = { 0,0,0,0,0 };

int main() {
  struct S1 *ps1 = (struct S1 *)array;
  int i, j;

  ps1->a = 1;
  ps1->b = 1;
  ps1->c = 1;
  ps1->d = 1;
  for (i = 0; i < 5; ++i) {
    for (j = 0; j < sizeof(int); ++j) {
      printf("%02x ", array[i*(sizeof(int))+j]);
    }
    printf(" ");
  }
  printf("\n");
  printf("sizeof S1 is %d\n", sizeof(struct S1));
}

