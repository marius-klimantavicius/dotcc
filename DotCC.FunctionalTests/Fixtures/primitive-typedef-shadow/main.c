#include <stdio.h>
typedef struct list { int value; } list;
void release(void *list);
list after_prototype;
void release(void *list) { ((int *)list)[0] = 42; }
list after_definition;
int read_tag(struct list *list) { return list->value; }

typedef struct Wrapper { int list; list *next; } Wrapper;
typedef void (*Consumer)(void *list);
void callback_scope(void (*consume)(void *list), list *value) { consume(value); }

typedef int action;
void invoke(void (*action)(int *), int *value) { action(value); }
action after_callback;
void set(int *p) { *p = 9; }
int local_callback(void) { int value=0; void (*action)(int *) = set; action(&value); return value; }

typedef int number;
struct Record { int number; number next; };
int local(void) { int number = 8; { int other = (int)number; number = other + 1; } return number; }

int main(void) {
    Consumer consume = release;
    consume(&after_prototype);
    callback_scope(release, &after_definition);
    invoke(set, &after_callback);
    Wrapper w = {7, &after_definition};
    struct Record record = {3, 4};
    number n = (number)local();
    printf("%d %d %d %d %d %d %d %d\n", read_tag(&after_prototype),
        read_tag(w.next), w.list, after_callback, local_callback(), n,
        record.number, record.next);
    return 0;
}
