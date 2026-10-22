/* Real popen/stdio host-boundary control, not database initialization. */
#include <stdio.h>

int locale_inventory_copy(const char *path)
{
    FILE *source = popen("locale -a", "r");
    FILE *target;
    int value, failed = 0, status;
    if (source == NULL) return 81;
    target = fopen(path, "wb");
    if (target == NULL) { pclose(source); return 82; }
    while ((value = fgetc(source)) != EOF)
        if (fputc(value, target) == EOF) { failed = 83; break; }
    if (ferror(source)) failed = 84;
    if (fclose(target) != 0) failed = 85;
    status = pclose(source);
    return failed != 0 ? failed : status;
}

#ifdef DOTCC_NATIVE_MAIN
int main(int argc, char **argv)
{
    return argc == 2 ? locale_inventory_copy(argv[1]) : 86;
}
#endif
