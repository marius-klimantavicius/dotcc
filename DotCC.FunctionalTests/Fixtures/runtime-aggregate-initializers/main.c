#define _GNU_SOURCE 1
#include <time.h>
#include <locale.h>
#include <stddef.h>
#include <stdio.h>

static struct timespec global = {4294967297L, 123456789L};
static struct timespec globals[2] = {{11, 22}, {33, 44}};
struct Wrapped { char prefix; struct timespec stamp; char tail; };

int main(void)
{
    struct timespec zero = {0, 0};
    struct timespec partial = {17};
    struct timespec local = {.tv_nsec = 123456789L, .tv_sec = 4294967297L};
    struct timespec array[3] = {{1, 2}, {3, 4}};
    struct Wrapped nested = {5, {6, 7}, 8};
    struct timespec compound = (struct timespec){9, 10};
    struct tm calendar = {1, 2, 3, 4, 5, 126, 6, 7, 0};
    struct tm zone = {.tm_gmtoff = 4294967298L, .tm_zone = "UTC"};
    struct lconv conventions = {.decimal_point = ".", .frac_digits = 2};
    printf("zero %ld %ld partial %ld %ld\n", zero.tv_sec, zero.tv_nsec, partial.tv_sec, partial.tv_nsec);
    printf("wide %ld %ld %lu %lu\n", global.tv_sec, local.tv_sec, sizeof(local.tv_sec), sizeof(zone.tm_gmtoff));
    printf("arrays %ld %ld %ld %ld\n", globals[1].tv_nsec, array[0].tv_nsec, array[1].tv_sec, array[2].tv_nsec);
    printf("nested %d %ld %ld %d compound %ld %ld\n", nested.prefix, nested.stamp.tv_sec, nested.stamp.tv_nsec, nested.tail, compound.tv_sec, compound.tv_nsec);
    printf("calendar %d %d %d %ld %s locale %s %d\n", calendar.tm_sec, calendar.tm_year, calendar.tm_yday, zone.tm_gmtoff, zone.tm_zone, conventions.decimal_point, conventions.frac_digits);
    printf("layout %lu %lu %lu %lu %lu\n", sizeof(struct timespec), _Alignof(struct timespec), offsetof(struct timespec, tv_nsec), offsetof(struct Wrapped, stamp), sizeof(struct Wrapped));
    printf("addresses %ld %ld %ld\n", (long)((char *)&local.tv_nsec - (char *)&local), (long)((char *)&nested.stamp - (char *)&nested), (long)((char *)&array[1] - (char *)&array[0]));
    printf("runtime %d %ld\n", timespec_get(&local, 0), local.tv_sec);
    return 0;
}
