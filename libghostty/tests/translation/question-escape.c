#include <stdio.h>

int main(void) {
    const char *text = "\?\?=";
    printf("%s %d %d %d\n", text, (int)text[0], (int)text[1], (int)text[2]);
    return 0;
}
