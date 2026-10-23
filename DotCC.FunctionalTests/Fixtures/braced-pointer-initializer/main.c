#include <stdio.h>
struct Value { int number; };
static struct Value global = { 7 };
static struct Value *static_pointer = { &global };
int main(void)
{
    struct Value *missing = { 0 };
    struct Value *present = { &global };
    const char *text = { "ready" };
    printf("%d %d %d %s\n", missing == 0, present->number, static_pointer->number, text);
    return 0;
}
