#include <stdio.h>
extern int fixture(void);
extern int memory_observation_count(void);
extern long long memory_observe(int);
extern int memory_event_count(void);
extern int memory_event(int);
int main(void) {
    printf("result=%d\n", fixture());
    printf("observations=%d\n", memory_observation_count());
    for (int index = 0; index < memory_observation_count(); ++index)
        printf("%d:%lld\n", index, memory_observe(index));
    printf("events=%d\n", memory_event_count());
    for (int index = 0; index < memory_event_count(); ++index)
        printf("event%d:%d\n", index, memory_event(index));
    return 0;
}
