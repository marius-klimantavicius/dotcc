#include <stdio.h>
static inline const char *message(void) { return "hello"; }
static inline unsigned const long count(void) { return 42; }
int main(void) { printf("%s %lu\n", message(), count()); return 0; }
