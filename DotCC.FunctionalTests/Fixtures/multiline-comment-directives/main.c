#include <stdio.h>
#define FIELDS(prefix) \
    int prefix##_left; \
    /* comment first line
       comment second line */ \
    unsigned prefix##_flag: 1; \
    int prefix##_right;
struct Pair { FIELDS(item) };
#define VALUE (4 /* block
comment */ + 5)
#if 1 /* block
comment */ && 0
#error inactive branch
#endif
#define STRINGIFY(x) #x
static char *spelling = STRINGIFY(one /* block
comment */ two);
static char *literal = "/* literal */";
#line 80 "comment-line-control.c"
static int observed_line = __LINE__;
int main(void) {
    struct Pair pair = {3,1,VALUE};
    printf("%d %d %u %s %c %d\n", pair.item_left, pair.item_right, pair.item_flag,
           spelling, literal[0], observed_line);
    return 0;
}
