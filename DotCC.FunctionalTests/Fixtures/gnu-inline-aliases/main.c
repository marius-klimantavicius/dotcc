#include <stdio.h>
#define INLINE __inline
static INLINE int increment(int n) { return n+1; }
static __inline__ int twice(int n) { return 2*n; }
int main(void) { printf("%d %d\n", increment(6), twice(9)); return 0; }
