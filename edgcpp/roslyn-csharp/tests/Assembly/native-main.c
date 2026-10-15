#include <stdio.h>
extern int assembled_entry(void);
int main(void) { int result = assembled_entry(); printf("%d\n", result); return result != 32; }
