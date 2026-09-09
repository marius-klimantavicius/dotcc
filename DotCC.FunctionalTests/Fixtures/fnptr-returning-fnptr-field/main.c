#include <stdio.h>
typedef struct Vfs Vfs;
struct Vfs {
    void (*(*lookup)(Vfs *, void *, const char *))(void);
};
static int count;
static void callback(void) { count += 7; }
static void (*lookup(Vfs *vfs, void *context, const char *name))(void) {
    return callback;
}
int main(void) {
    Vfs vfs = { lookup };
    vfs.lookup(&vfs, 0, "callback")();
    printf("%d\n", count);
    return 0;
}
