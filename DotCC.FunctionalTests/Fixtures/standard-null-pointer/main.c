#include <stddef.h>
#include <stdlib.h>
#include <stdio.h>
#include <locale.h>
#include <time.h>

typedef int (*Callback)(int);
static int value = 41;
static int *global_pointer = NULL;
static Callback global_callback = NULL;
static int add_one(int input) { return input + 1; }
static int *pick_pointer(int choose_null, int *pointer) { return choose_null ? NULL : pointer; }
static int *reverse_pointer(int choose_value, int *pointer) { return choose_value ? pointer : NULL; }
static Callback pick_callback(int choose_null, Callback callback) { return choose_null ? NULL : callback; }
static Callback reverse_callback(int choose_value, Callback callback) { return choose_value ? callback : NULL; }
static void *allocate(int fail) { return fail ? NULL : malloc(4); }
static int accept_pointer(const int *pointer) { return pointer == NULL; }
static int accept_callback(Callback callback) { return callback == NULL; }

int main(void) {
    int *pointer = NULL;
    Callback callback = NULL;
    void *allocation = allocate(0);
    printf("%d %d %d %d %d\n", (int)(sizeof(NULL) == sizeof(void *)),
           _Generic(NULL, void *: 1, default: 0), pointer == NULL,
           callback == NULL, global_pointer == NULL && global_callback == NULL);
    printf("%d %d %d %d\n", pick_pointer(1, &value) == NULL,
           *pick_pointer(0, &value), reverse_pointer(0, &value) == NULL,
           *reverse_pointer(1, &value));
    printf("%d %d %d %d\n", pick_callback(1, add_one) == NULL,
           pick_callback(0, add_one)(41), reverse_callback(0, add_one) == NULL,
           reverse_callback(1, add_one)(41));
    printf("%d %d %d %d\n", accept_pointer(NULL), accept_callback(NULL),
           allocate(1) == NULL, allocation != NULL);
    free(allocation);
    return 0;
}
