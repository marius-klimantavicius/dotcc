#include <stdio.h>
#include <threads.h>
#include <string.h>

struct debug_info { const char *text; const char *source; int count; };
__thread struct debug_info current = { .text = "ready", .source = "file" };
static int worker(void *unused) {
    if (strcmp(current.text, "ready") || strcmp(current.source, "file") || current.count) return 99;
    current.text = "worker";
    current.count = 8;
    return current.count;
}
int main(void) {
    current.count = 3;
    thrd_t thread;
    int result;
    thrd_create(&thread, worker, 0);
    thrd_join(thread, &result);
    printf("%s %s %d %d\n", current.text, current.source, current.count, result);
    return result != 8 || current.count != 3;
}
