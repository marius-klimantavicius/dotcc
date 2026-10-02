#include <stdio.h>

export template<class T> void f() {
#ifdef M
	printf("def\n");
#else
	printf("undef\n");
#endif
}

