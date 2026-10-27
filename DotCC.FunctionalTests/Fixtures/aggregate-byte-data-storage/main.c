#include <stdio.h>
#include "probe.h"

int main(void) {
    for (int table=0; table<2; ++table) {
        int size = storage_size(table);
        printf("record=%d bytes=%d:", table, size);
        for (int i=0; i<8; ++i) printf("%02x", (unsigned)storage_byte(table, i));
        printf(":");
        for (int i=size-12; i<size; ++i) printf("%02x", (unsigned)storage_byte(table, i));
        printf("\n");
    }
    int initial = storage_static_check(-17), tls_initial = storage_tls_check(73);
    int static_alignment = storage_static_alignment(), tls_alignment = storage_tls_alignment();
    storage_static_set(101);
    int mutation = storage_static_check(101);
    int loop_first = storage_loop(5, 2), loop_second = storage_loop(4, 1);
    unsigned tls_first = storage_tls_step(), tls_second = storage_tls_step();
    int tls_final = storage_tls_check(75);
    printf("initial=%d tls=%d align=%d,%d mutation=%d loops=%d,%d steps=%u,%u final=%d\n",
        initial, tls_initial, static_alignment, tls_alignment, mutation, loop_first, loop_second,
        tls_first, tls_second, tls_final);
    return initial && tls_initial && static_alignment && tls_alignment && mutation &&
        loop_first && loop_second && tls_first == 73 && tls_second == 74 && tls_final ? 0 : 1;
}
