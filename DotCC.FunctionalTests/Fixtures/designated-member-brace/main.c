#include <stdio.h>
typedef struct { unsigned short type; void *data; } Extension;
typedef struct { int a; int b; } Pair;
typedef struct { Extension extensions[3]; Pair pair; int matrix[2][2]; int scalar; } Hello;
Hello global = {.extensions = {{65535}}, .pair = {.b = {7}}, .matrix = {{1},{2,3}}, .scalar = {8}};
int persistent(void) {
    static Hello saved = {.pair = {.a = {10}}};
    return saved.pair.a++;
}
int main(void) {
    Hello hello;
    Hello *pointer = &hello;
    *pointer = (Hello){.extensions = {{65535}}};
    printf("zero %d %d %d %d %d %d %d\n", hello.extensions[0].type, hello.extensions[0].data == NULL, hello.extensions[1].type, hello.extensions[2].data == NULL, hello.pair.a, hello.matrix[1][1], hello.scalar);
    hello = global;
    printf("nested %d %d %d %d %d %d %d %d %d\n", hello.extensions[0].type, hello.extensions[1].type, hello.pair.a, hello.pair.b, hello.matrix[0][0], hello.matrix[0][1], hello.matrix[1][0], hello.matrix[1][1], hello.scalar);
    int first = persistent(), second = persistent();
    printf("static %d %d\n", first, second);
    return 0;
}
