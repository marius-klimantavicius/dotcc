#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <time.h>

/* Real shared-header/service reduction. The wide advertised capacity does not
   change the short output; no large allocation or native Python is involved. */
int main(void)
{
    time_t epoch = 0;
    struct tm *calendar = gmtime(&epoch);
    unsigned char storage[66];
    char *output = (char *)(storage + 1);
    const char *expected = "1970-01-01 00:00:00";
    char empty_format[] = {0};
    size_t capacity = ((size_t)1 << 31) + 65;
    size_t ordinary, wide, tight, empty;
    int failures = 0;
    if (!calendar) return 20;
    memset(storage, 0x5a, sizeof(storage));
    ordinary = strftime(output, 64, "%Y-%m-%d %H:%M:%S", calendar);
    if (ordinary != 19 || strcmp(output, expected) || output[19] != 0) failures++;
    if (storage[0] != 0x5a || storage[65] != 0x5a) failures++;
    memset(storage, 0x5a, sizeof(storage));
    wide = strftime(output, capacity, "%Y-%m-%d %H:%M:%S", calendar);
    if (wide != 19 || strcmp(output, expected) || output[19] != 0) failures++;
    if (storage[0] != 0x5a || storage[65] != 0x5a) failures++;
    memset(storage, 0x5a, sizeof(storage));
    tight = strftime(output, 4, "%Y", calendar);
    if (tight != 0 || storage[0] != 0x5a || storage[65] != 0x5a) failures++;
    empty = strftime(output, 1, empty_format, calendar);
    if (empty != 0 || output[0] != 0) failures++;
    printf("strftime ordinary=%lu wide=%lu tight=%lu empty=%lu failures=%d\n",
           (unsigned long)ordinary, (unsigned long)wide,
           (unsigned long)tight, (unsigned long)empty, failures);
    return failures ? 23 : 0;
}
