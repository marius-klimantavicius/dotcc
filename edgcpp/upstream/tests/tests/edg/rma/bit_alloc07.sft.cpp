//options_all:-r -x -tused
//options: --strict;cn:;rp

#ifdef __cplusplus
extern "C" void printf(char*, ...);
#else
extern void printf();
#endif /* __cplusplus */

struct S1 {
  char a:1;
  short :4;
  char b:1;
} x;

struct S2 {
  char a:1;
  short :0;
  char b:1;
} y;
main() { }
#if 0
#define INT_COUNT 4
static unsigned char array[INT_COUNT*sizeof(int)];

void clear_array() {
  int i;
  for (i = 0; i < INT_COUNT*sizeof(int); ++i) { array[i] = 0; }
}

void disp_array(int struct_size) {
  int i, j;
  for (i = 0; i < INT_COUNT; ++i) {
    for (j = 0; j < sizeof(int); ++j) {
      printf("%02x ", array[i*(sizeof(int))+j]);
    }
    printf(" ");
  }
  printf("\n");
  printf("size of struct is %d\n", struct_size);
}

int main() {
  clear_array();
  struct S1 *ps1 = (struct S1 *)array;
  ps1->a = 1;
  ps1->b = 1;
  disp_array(sizeof(struct S1));

  clear_array();
  struct S2 *ps2 = (struct S2 *)array;
  ps2->a = 1;
  ps2->b = 1;
  disp_array(sizeof(struct S2));
}
#endif /* if 0 */

