#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <sys/stat.h>
#include <unistd.h>
#include <stdbool.h>

extern int fsync_fname(const char *path, bool directory);
extern int fsync_parent_path(const char *path);
extern int durable_rename(const char *source, const char *target);

int main(int argc, char **argv)
{
    char source[4096], target[4096], bytes[128] = {0};
    if (argc != 2) return 1;
    snprintf(source, sizeof(source), "%s/source", argv[1]);
    snprintf(target, sizeof(target), "%s/target", argv[1]);
    FILE *file = fopen(source, "wb");
    if (!file) return 2;
    fputs("original complete PostgreSQL durable_rename UTF-8 Ω", file); fclose(file);
    file = fopen(target, "wb"); if (!file) return 3;
    fputs("previous bytes", file); fclose(file);
    if (fsync_fname(argv[1], true) != 0 || fsync_parent_path(target) != 0) return 4;
    if (durable_rename(source, target) != 0 || access(source, F_OK) == 0) return 5;
    file = fopen(target, "rb"); if (!file) return 6;
    size_t count = fread(bytes, 1, sizeof(bytes) - 1, file); fclose(file);
    if (count != strlen("original complete PostgreSQL durable_rename UTF-8 Ω")) return 12;
    if (strcmp(bytes, "original complete PostgreSQL durable_rename UTF-8 Ω")) return 7;
    unlink(target);
    file = fopen(source, "wb"); if (!file) return 8;
    fputs("new target path", file); fclose(file);
    if (durable_rename(source, target) != 0 || access(source, F_OK) == 0) return 9;
    file = fopen(target, "rb"); if (!file) return 10;
    memset(bytes, 0, sizeof(bytes)); count = fread(bytes, 1, sizeof(bytes) - 1, file); fclose(file);
    if (count != strlen("new target path")) return 13;
    if (strcmp(bytes, "new target path")) return 11;
    puts("PASS original native complete common/file_utils.c durable rename, existing and absent targets");
    return 0;
}
