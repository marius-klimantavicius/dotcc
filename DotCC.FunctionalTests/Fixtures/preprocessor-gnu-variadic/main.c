#include <stdio.h>
#define LOG(fmt, ...) printf((fmt), ##__VA_ARGS__)
#define TRACE(name, fmt, ...) LOG((fmt " [" #name "] %s:%d\n"), ##__VA_ARGS__, __FILE__, __LINE__)
#define EVENT(name, fmt, ...) TRACE(name, fmt, ##__VA_ARGS__)
#define VALUE 42
#define PAIR "pair", VALUE
int main(void) {
#line 100 "logging.c"
    EVENT(AllocFailure, "%s %d", "key", VALUE);
    EVENT(Ready, "ready");
    EVENT(Pair, "%s %d", PAIR);
    LOG("plain\n");
    return 0;
}
