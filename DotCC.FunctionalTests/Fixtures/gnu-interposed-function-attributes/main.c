#include <stdio.h>
static inline char *
__attribute__((no_instrument_function))
identity(char *value) { return value; }
typedef int Integer;
Integer * __attribute__((noinline)) integer_identity(Integer *value) { return value; }
int main(void) {
    char text[] = "callback";
    Integer value = 42;
    char *(*callback)(char *) = identity;
    printf("%s %d %d\n", callback(text), *integer_identity(&value), identity(text) == text);
    return 0;
}
