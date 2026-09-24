#include <stdio.h>
#define CONFIG(default, enum) ((default) + (enum))
#define ENTRY(default, enum) {.value=(default), .kind=(enum)}
#define PASTE(default, enum) default ## enum
#define STRING(default) #default
#define FORWARD(default) STRING(default)
#define VALUE 42
#define answer_value 9
#if CONFIG(40, 2) != 42
#error keyword macro arguments were not substituted
#endif
struct Entry { int value; int kind; } entries[] = {ENTRY(VALUE, 7)};
int main(void) {
    printf("%d %d %d %d %s %s\n", CONFIG(40,2), entries[0].value, entries[0].kind,
        PASTE(answer_,value), STRING(VALUE), FORWARD(VALUE));
    return 0;
}
