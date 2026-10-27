#include <stdio.h>
#include "probe.h"

int main(void) {
    for (int table=0; table<aggregate_count(); ++table) {
        int size=aggregate_row_size(table), rows=aggregate_rows(table);
        unsigned char *bytes=aggregate_bytes(table);
        printf("%d %d %d:", table, size, rows);
        for (int i=0; i<2*size; ++i) printf("%02x", (unsigned)bytes[i]);
        printf(":");
        for (int i=(rows-1)*size; i<rows*size; ++i) printf("%02x", (unsigned)bytes[i]);
        printf("\n");
    }
    int values=aggregate_values(), gaps=aggregate_zero_gaps(), stores=aggregate_wide_store();
    int identity=aggregate_mutate();
    printf("values=%d zero=%d stores=%d identity=%d\n", values, gaps, stores, identity);
    int local_first=aggregate_local(), local_second=aggregate_local();
    int aligned_first=aggregate_aligned_local(), aligned_second=aggregate_aligned_local();
    int tls_first=aggregate_tls_step(), tls_second=aggregate_tls_step();
    int aligned_global=aggregate_aligned_global();
    printf("locals=%d,%d aligned=%d,%d global=%d tls=%d,%d\n", local_first, local_second,
        aligned_first, aligned_second, aligned_global, tls_first, tls_second);
    return values && gaps && stores && identity && local_first && local_second
        && aligned_first && aligned_second && aligned_global && tls_first == 17 && tls_second == 18 ? 0 : 1;
}
