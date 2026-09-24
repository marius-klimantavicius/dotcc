#include <stdio.h>
#define CAT_(name,n) name##n
#define CAT(name,n) CAT_(name,n)
#define CALL(name, n, ...) CAT(name,n)(__VA_ARGS__)
#define F2(fmt,value) fmt
#define V2(fmt,value) value
#define PAIR(...) CALL(F,2,__VA_ARGS__), CALL(V,2,__VA_ARGS__)
int main(void) { printf("prefix " PAIR("%d\n",42)); return 0; }
