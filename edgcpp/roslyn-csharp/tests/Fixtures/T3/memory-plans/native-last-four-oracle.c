#include <stdio.h>
extern int fixture(void);
extern int last_four_count(void);
extern long long last_four_observe(int);
extern int last_four_event_count(void);
extern int last_four_event(int);
int main(void) {
    printf("result=%d\n", fixture());
    printf("observations=%d\n", last_four_count());
    for (int index = 0; index < last_four_count(); ++index)
        printf("%d:%lld\n", index, last_four_observe(index));
    printf("events=%d\n", last_four_event_count());
    for (int index = 0; index < last_four_event_count(); ++index)
        printf("event%d:%d\n", index, last_four_event(index));
    return 0;
}
