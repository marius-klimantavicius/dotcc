/* SOURCE-ONLY next control. Native system headers and unchanged V21 emitted
 * headers must run the exact same C after coordinated heavy-lane release.
 * fstat(pipe) is a prerequisite for exposing HAVE_PIPE to CPython FileIO.
 * This fixture is UNRUN; it records no native or translated success. */
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <sys/stat.h>
#include <unistd.h>

static int checks, failures;
static void equal(const char *name, long actual, long expected)
{
    checks++;
    if (actual != expected) {
        failures++;
        printf("FAIL %s actual=%ld expected=%ld\n", name, actual, expected);
    }
}

int main(void)
{
    int descriptors[2];
    int result = pipe(descriptors);
    equal("pipe", result, 0);
    if (result != 0) return 1;
    for (int end = 0; end < 2; end++) {
        struct stat metadata;
        result = fstat(descriptors[end], &metadata);
        equal("fstat-pipe", result, 0);
        if (result == 0) equal("fifo-mode", S_ISFIFO(metadata.st_mode) != 0, 1);
        errno = 0;
        equal("seek-pipe", lseek(descriptors[end], 0, SEEK_CUR), -1);
        equal("seek-errno", errno, ESPIPE);
        int flags = fcntl(descriptors[end], F_GETFL);
        equal("get-flags", flags >= 0, 1);
        equal("set-nonblock", fcntl(descriptors[end], F_SETFL, flags | O_NONBLOCK), 0);
        equal("nonblock-roundtrip", (fcntl(descriptors[end], F_GETFL) & O_NONBLOCK) != 0, 1);
    }
    unsigned char byte = 0, marker = 0x5a;
    errno = 0;
    equal("empty-nonblock-read", read(descriptors[0], &byte, 1), -1);
    equal("empty-read-errno", errno, EAGAIN);
    equal("pipe-write", write(descriptors[1], &marker, 1), 1);
    equal("pipe-read", read(descriptors[0], &byte, 1), 1);
    equal("pipe-byte", byte, marker);
    equal("close-writer", close(descriptors[1]), 0);
    equal("drained-eof", read(descriptors[0], &byte, 1), 0);
    equal("close-reader", close(descriptors[0]), 0);
    printf("SUMMARY checks=%d failures=%d\n", checks, failures);
    return failures ? 1 : 0;
}
