#include <stdio.h>
#include <time.h>
struct DateTime { int value; };
int main(void) {
    time_t epoch = 0;
    struct tm *broken_down = gmtime(&epoch);
    char text[32];
    struct DateTime local = {42};
    if (!broken_down || strftime(text, sizeof(text), "%Y-%m-%d %H:%M:%S", broken_down) != 19) return 1;
    printf("%s %d\n", text, local.value);
    return 0;
}
