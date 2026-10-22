/* Reduction of the first remaining declaration in actual pglitec.c. */
#include <setjmp.h>
volatile sigjmp_buf postgresmain_sigjmp_buf;
int main(void) { return 0; }
