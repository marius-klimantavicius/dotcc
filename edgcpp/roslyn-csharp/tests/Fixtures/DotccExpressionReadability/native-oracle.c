#include <stdio.h>

int fixture(void);
int expression_observation_count(void);
long long expression_observe(int index);
int expression_event_count(void);
int expression_event(int index);

int main(void) {
    printf("result=%d\nobservations=%d\n", fixture(), expression_observation_count());
    for (int index = 0; index < expression_observation_count(); ++index)
        printf("%d:%lld\n", index, expression_observe(index));
    printf("events=%d\n", expression_event_count());
    for (int index = 0; index < expression_event_count(); ++index)
        printf("event%d:%d\n", index, expression_event(index));
    return 0;
}
