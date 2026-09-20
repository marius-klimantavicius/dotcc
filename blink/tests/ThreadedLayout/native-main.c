#include <stdio.h>
int ThreadedLayoutCount(void);
const char *ThreadedLayoutName(int);
unsigned long ThreadedLayoutValue(int);
int main(void) {
  for(int i=0;i<ThreadedLayoutCount();++i)
    printf("%s %lu\n",ThreadedLayoutName(i),ThreadedLayoutValue(i));
  return 0;
}
