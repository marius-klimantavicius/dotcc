/* Sequential test driver. Terminal behavior and assertions stay in upstream C. */
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>

size_t dotcc_upstream_count(void);
const uint8_t *dotcc_upstream_name(size_t index, size_t *len);
int dotcc_upstream_run(size_t index);
int dotcc_upstream_host_finish(size_t count);
size_t dotcc_upstream_io_live(void);

int main(void) {
    size_t count = dotcc_upstream_count();
    int resource_failure = 0;
    for (size_t index = 0; index < count; index++) {
        size_t len;
        const uint8_t *name = dotcc_upstream_name(index, &len);
        fprintf(stderr, "RUN %.*s\n", (int)len, name);
        dotcc_upstream_run(index);
        if (dotcc_upstream_io_live() != 0) {
            fprintf(stderr, "FAIL fixture resource leak after %.*s\n", (int)len, name);
            resource_failure = 1;
        }
    }
    return dotcc_upstream_host_finish(count) || resource_failure ? 1 : 0;
}
