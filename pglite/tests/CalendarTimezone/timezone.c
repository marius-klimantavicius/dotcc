#define _GNU_SOURCE 1
#include <time.h>
#include <locale.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>

#define REQUIRE(value) do { if (!(value)) return __LINE__; } while (0)

static int supplied_zone(void)
{
    char output[80];
    struct tm value = {0};
    long offsets[] = {0, 19800, -12600, 20700, -45900, 30, -30, 19859, -12659};
    char *expected[] = {"+0000", "+0530", "-0330", "+0545", "-1245", "+0000", "-0000", "+0530", "-0330"};
    int i;
    value.tm_zone = "CALLER-0530";
    value.tm_isdst = 0;
    REQUIRE(strftime(output, sizeof output, "%Z", &value) == 11);
    REQUIRE(strcmp(output, "CALLER-0530") == 0);
    value.tm_isdst = -1;
    REQUIRE(strftime(output, sizeof output, "%Z|%z", &value) == 12);
    REQUIRE(strcmp(output, "CALLER-0530|") == 0);
    value.tm_zone = NULL;
    REQUIRE(strftime(output, sizeof output, "[%Z|%z]", &value) == 3);
    REQUIRE(strcmp(output, "[|]") == 0);
    value.tm_zone = "";
    value.tm_isdst = -1;
    REQUIRE(strftime(output, sizeof output, "%Z", &value) == 0 && output[0] == 0);
    for (i = 0; i < 2; ++i)
    {
        value.tm_isdst = i;
        value.tm_zone = NULL;
        REQUIRE(strftime(output, sizeof output, "[%Z]", &value) > 2);
        printf("fallback:%d:%s\n", i, output);
        value.tm_zone = "";
        REQUIRE(strftime(output, sizeof output, "[%Z]", &value) > 2);
        printf("empty:%d:%s\n", i, output);
    }
    for (i = 0; i < 9; ++i)
    {
        value.tm_gmtoff = offsets[i];
        value.tm_isdst = i % 2;
        REQUIRE(strftime(output, sizeof output, "%z", &value) == 5);
        REQUIRE(strcmp(output, expected[i]) == 0);
        printf("offset:%ld:%s\n", offsets[i], output);
    }
    value.tm_isdst = -1;
    REQUIRE(strftime(output, sizeof output, "[%z]", &value) == 2);
    REQUIRE(strcmp(output, "[]") == 0);
    value.tm_zone = "CALLER-0530";
    output[0] = 'X';
    REQUIRE(strftime(output, 11, "%Z", &value) == 0);
    REQUIRE(strftime(output, 12, "%Z", &value) == 11 && output[11] == 0);
    {
        locale_t locale = newlocale(LC_TIME_MASK, "C", NULL);
        REQUIRE(locale != NULL);
        value.tm_isdst = 0;
        value.tm_gmtoff = 20700;
        REQUIRE(strftime_l(output, sizeof output, "%Z|%z", &value, locale) == 17);
        REQUIRE(strcmp(output, "CALLER-0530|+0545") == 0);
        freelocale(locale);
    }
    return 0;
}

int dtz_run(void)
{
    time_t timestamps[] = {0, 1705320000, 1721044800, 1730611800, 1730615400};
    int i, status = supplied_zone();
    if (status != 0) return status;
    printf("layout:%lu:%lu:%lu\n", (unsigned long)sizeof(struct tm),
           (unsigned long)offsetof(struct tm, tm_gmtoff), (unsigned long)offsetof(struct tm, tm_zone));
    for (i = 0; i < 5; ++i)
    {
        struct tm local, normalized, utc;
        char output[100];
        time_t epoch = timestamps[i];
        REQUIRE(localtime_r(&epoch, &local) == &local);
        REQUIRE(local.tm_zone != NULL && local.tm_zone[0] != 0);
        REQUIRE(strftime(output, sizeof output, "%Y-%m-%d %H:%M:%S|%Z|%z", &local) != 0);
        printf("local:%ld:%s:%ld:%d\n", (long)epoch, output, local.tm_gmtoff, local.tm_isdst);
        normalized = local;
        REQUIRE(mktime(&normalized) == epoch);
        REQUIRE(normalized.tm_gmtoff == local.tm_gmtoff && normalized.tm_isdst == local.tm_isdst);
        REQUIRE(strcmp(normalized.tm_zone, local.tm_zone) == 0);
        REQUIRE(gmtime_r(&epoch, &utc) == &utc);
        REQUIRE(utc.tm_gmtoff == 0 && utc.tm_isdst == 0);
        REQUIRE(strcmp(utc.tm_zone, "GMT") == 0 || strcmp(utc.tm_zone, "UTC") == 0);
        REQUIRE(strftime(output, sizeof output, "%z", &utc) != 0);
        printf("utc:%s\n", output);
    }
    return 0;
}

#ifdef DTZ_NATIVE_MAIN
int main(void)
{
    int status = dtz_run();
    if (status) fprintf(stderr, "timezone assertion failed at line %d\n", status);
    return status ? 1 : 0;
}
#endif
