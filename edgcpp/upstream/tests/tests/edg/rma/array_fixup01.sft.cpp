//options_all:-r -x -tused
//options: --strict;cn:;cp

/* Multi-dimensioned array of incomplete struct (an extension in C). */
extern "C" void printf(const char *, ...);
struct A;
typedef struct A arr[5][6];
struct A { int i; };
main () {
  if (sizeof(arr) == 5*6*sizeof(int)) printf("pass\n"); else printf("fail\n");
  return 0;
}

