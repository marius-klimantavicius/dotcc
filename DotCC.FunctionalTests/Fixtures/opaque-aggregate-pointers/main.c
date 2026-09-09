#include <stdio.h>
typedef struct Handle Handle;
struct Device;
typedef union Token Token;
typedef struct Later Later;
Later later_value;
struct Later { int value; };
typedef int (*Inspect)(Handle *, struct Device *, Token *);
static Handle *saved;
static int inspect(Handle *h, struct Device *d, Token *t) {
    return (h == 0) + (d == 0) + (t == 0);
}
static Handle *echo(Handle *handle) { return handle; }
static int invoke(Inspect callback) { return callback(0, 0, 0); }
int main(void) {
    Later storage = {42};
    saved = (Handle *)&storage;
    printf("%d %d %d %d\n", echo(saved) == saved, invoke(inspect), ((Later *)saved)->value, (int)sizeof(later_value));
    return 0;
}
