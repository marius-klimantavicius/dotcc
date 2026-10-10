#include <stdio.h>
extern "C" unsigned long long duckdb_lifetime_case(int, int);
int main() {
    int failures[] = {0, 1, 2, 3, 10, 12};
    for (int scenario = 0; scenario != 4; ++scenario)
        for (int fail : failures)
            printf("%d %d %llu\n", scenario, fail, duckdb_lifetime_case(scenario, fail));
}
