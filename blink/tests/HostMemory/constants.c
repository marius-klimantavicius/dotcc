#define _DEFAULT_SOURCE 1
#include <stdio.h>
#include <sys/mman.h>
int main(void) {
  printf("PROT_NONE %d\n", PROT_NONE);
  printf("PROT_READ %d\n", PROT_READ);
  printf("PROT_WRITE %d\n", PROT_WRITE);
  printf("PROT_EXEC %d\n", PROT_EXEC);
  printf("MAP_SHARED %d\n", MAP_SHARED);
  printf("MAP_PRIVATE %d\n", MAP_PRIVATE);
  printf("MAP_FIXED %d\n", MAP_FIXED);
  printf("MAP_ANONYMOUS %d\n", MAP_ANONYMOUS);
  printf("MS_ASYNC %d\n", MS_ASYNC);
  printf("MS_INVALIDATE %d\n", MS_INVALIDATE);
  printf("MS_SYNC %d\n", MS_SYNC);
  return MAP_FAILED != (void *)-1;
}
