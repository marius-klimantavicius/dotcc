/* Native and emitted control: freopen must drain setvbuf output before
   replacing the backing file. Caller storage remains alive through fclose. */
#include <stdio.h>
#include <string.h>

int main(void) {
    char buffer[32];
    char old_bytes[4] = {0}, new_bytes[4] = {0};
    FILE *file = fopen("r6-stdio-old", "w+");
    if (file == NULL || setvbuf(file, buffer, _IOFBF, sizeof buffer) != 0) return 1;
    if (fputc('A', file) != 'A') return 2;
    if (freopen("r6-stdio-new", "w+", file) != file) return 3;
    if (fputc('B', file) != 'B' || fclose(file) != 0) return 4;
    file = fopen("r6-stdio-old", "r");
    if (file == NULL || fread(old_bytes, 1, 3, file) != 1 || fclose(file) != 0) return 5;
    file = fopen("r6-stdio-new", "r");
    if (file == NULL || fread(new_bytes, 1, 3, file) != 1 || fclose(file) != 0) return 6;
    if (strcmp(old_bytes, "A") != 0 || strcmp(new_bytes, "B") != 0) return 7;
    if (remove("r6-stdio-old") != 0 || remove("r6-stdio-new") != 0) return 8;
    puts("stdio-reopen passed old=A new=B");
    return 0;
}
