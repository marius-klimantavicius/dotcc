#include <stdio.h>
#include <stddef.h>
#include <sys/types.h>
#include <sys/uio.h>
#include <errno.h>

int main(void) {
    _Static_assert(sizeof(off_t) == 8, "64-bit file positions");
    _Static_assert(sizeof(struct iovec) == 16, "portable LP64 iovec");
    _Static_assert(offsetof(struct iovec, iov_len) == 8, "portable LP64 iovec offset");
    FILE *file = tmpfile();
    if (!file) return 1;
    off_t far = ((off_t)1 << 33) + 17;
    if (fseeko(file, far, SEEK_SET) || ftello(file) != far) return 2;
    if (fputc(0x5a, file) != 0x5a || ftello(file) != far + 1) return 3;
    if (fseeko(file, -1, SEEK_END) || ftello(file) != far) return 4;
    if (fgetc(file) != 0x5a || fgetc(file) != EOF || !feof(file)) return 5;
    if (fseeko(file, -1, SEEK_CUR) || feof(file) || fgetc(file) != 0x5a) return 6;
    /* Failed seeks preserve a valid stream and the original file position. */
    errno = 0;
    if (fseeko(file, -1, SEEK_SET) == 0 || errno != EINVAL) return 7;
    if (ftello(file) != far + 1) return 8;
    if (fseeko(file, 0, SEEK_SET) || fgetc(file) != 0) return 9;
    /* Legacy and POSIX spellings share the same stream position. */
    if (fseek(file, 3, SEEK_SET) || ftello(file) != 3) return 10;
    if (fseeko(file, 2, SEEK_CUR) || ftell(file) != 5) return 11;
    if (fclose(file)) return 12;
    puts("64-bit stream positions: passed");
    return 0;
}
