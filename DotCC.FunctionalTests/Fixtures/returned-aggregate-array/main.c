#include <stdio.h>
#include <stdalign.h>
#include <stdint.h>
struct Text { char text[4]; };
struct Nested { struct Text inner; };
struct Aligned { alignas(16) char text[4]; };
static int calls;
static struct Text make_text(void) { struct Text value = {{'a', 'b', 'c', 0}}; calls++; return value; }
static struct Nested make_nested(void) { struct Nested value = {{{'d', 'e', 'f', 0}}}; calls++; return value; }
static struct Aligned make_aligned(void) { struct Aligned value = {{'g', 'h', 'i', 0}}; calls++; return value; }
static void consume(const char *text) { printf("%s\n", text); }
static void consume_aligned(const char *text) { printf("%s %d\n", text, (int)((uintptr_t)text & 15)); }
int main(void) {
    consume(make_text().text);
    consume(make_nested().inner.text);
    consume_aligned(make_aligned().text);
    printf("calls %d\n", calls);
    return 0;
}
