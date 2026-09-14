#include <stdio.h>
#include <unistd.h>
int main(void) { printf("CLK_TCK=%d NGROUPS_MAX=%d PAGESIZE=%d\n", _SC_CLK_TCK, _SC_NGROUPS_MAX, _SC_PAGESIZE); }
