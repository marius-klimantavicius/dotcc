#include <stdio.h>
#define putByte(A,B) ((unsigned char)(((unsigned)(B)<128)?(*(A)=(unsigned char)(B)),1:2))
int main(void) {
    unsigned char byte = 0;
    int a = 0, b = 0;
    int first = 1 ? a = 10, b = 20, a + b : 99;
    int second = 0 ? a = 100, b = 200, a + b : 7;
    int third = 1 ? 0 ? (a += 3) : (a += 4), a + 2 : 5;
    int encoded = putByte(&byte, 42);
    printf("%d %d %d %d %d %d %d\n", first, second, third, a, b, encoded, byte);
    return 0;
}
