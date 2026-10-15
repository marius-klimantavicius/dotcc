#include <stdio.h>

#ifndef VALUE_COUNT
#error VALUE_COUNT must name the fixture observation count
#endif

extern int fixture(void);
extern int lifetime_count(void);
extern int lifetime_overflow(void);
extern int lifetime_live(void);
extern int lifetime_event(int index);
extern int lifetime_value(int index);

int main(void)
{
    int failed = 0;
    printf("{\"runs\":[");
    for (int run = 0; run < 2; ++run) {
        int result = fixture();
        int count = lifetime_count();
        int overflow = lifetime_overflow();
        int live = lifetime_live();
        if (run != 0) printf(",");
        printf("{\"result\":%d,\"overflow\":%d,\"live\":%d,\"values\":[",
               result, overflow, live);
        for (int index = 0; index < VALUE_COUNT; ++index) {
            if (index != 0) printf(",");
            printf("%d", lifetime_value(index));
        }
        printf("],\"events\":[");
        if (count < 0 || count > 64) failed = 1;
        for (int index = 0; index < count && index < 64; ++index) {
            if (index != 0) printf(",");
            printf("%d", lifetime_event(index));
        }
        printf("]}");
        if (overflow != 0 || live != 0 || lifetime_value(VALUE_COUNT - 1) != 0)
            failed = 1;
    }
    printf("]}\n");
    return failed;
}
