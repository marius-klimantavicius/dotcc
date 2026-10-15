/* Oracle-only source; never passed to the exporter or linked into generated execution. */
#include <stdio.h>
#include <stdlib.h>
int fixture(void);
int sequence_count(void);
int sequence_event(int index);
int sequence_value(int index);
int main(int argc, char **argv) {
    int result = fixture();
    int count = sequence_count();
    int values = argc == 2 ? atoi(argv[1]) : 0;
    printf("result=%d\nevents=%d\n", result, count);
    for (int index = 0; index < count; ++index) printf("event:%d:%d\n", index, sequence_event(index));
    for (int index = 0; index < values; ++index) printf("value:%d:%d\n", index, sequence_value(index));
    return 0;
}
