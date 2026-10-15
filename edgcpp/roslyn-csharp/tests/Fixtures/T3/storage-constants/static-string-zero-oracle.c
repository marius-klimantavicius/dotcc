/* Independent native reference process only; never a generated dependency. */
#include <stdio.h>
extern int fixture(void);
int main(void) { printf("%d\n", fixture()); printf("%d\n", fixture()); return 0; }
