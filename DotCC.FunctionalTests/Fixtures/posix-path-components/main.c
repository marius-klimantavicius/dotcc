#include <libgen.h>
#include <string.h>
#include <stdio.h>

static void check(const char *input) {
    char directory[80], component[80];
    strcpy(directory, input);
    strcpy(component, input);
    printf("[%s] [%s]\n", dirname(directory), basename(component));
}

int main(void) {
    check("");
    check("/");
    check("//");
    check("///");
    check("a");
    check("a///");
    check("a//b///");
    check("/a/b");
    check("//a");
    check("///a");
    check("//a/b");
    check(".");
    check("..");
    check("a/../b");
    check("a\\b");
    printf("[%s] [%s]\n", dirname(NULL), basename(NULL));
    return 0;
}
