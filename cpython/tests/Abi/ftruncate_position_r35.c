/* Exact same source is reserved for the next normal compiler capture's emitted
 * control. Native compilation uses system headers; DotCC uses its own headers. */
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <sys/stat.h>
#include <sys/socket.h>
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

static void resize_case(int fd, long initial, long position, long length)
{
    struct stat status;
    unsigned char marker = 0x5a, observed = 0;
    equal("initial-length", ftruncate(fd, initial), 0);
    equal("set-position", lseek(fd, position, SEEK_SET), position);
    errno = 123;
    equal("resize", ftruncate(fd, length), 0);
    equal("success-errno", errno, 123);
    equal("preserved-position", lseek(fd, 0, SEEK_CUR), position);
    equal("stat", fstat(fd, &status), 0);
    equal("length", status.st_size, length);
    equal("write", write(fd, &marker, 1), 1);
    equal("write-position", lseek(fd, 0, SEEK_CUR), position + 1);
    equal("stat-after-write", fstat(fd, &status), 0);
    equal("length-after-write", status.st_size, length > position ? length : position + 1);
    equal("seek-marker", lseek(fd, position, SEEK_SET), position);
    equal("read-marker", read(fd, &observed, 1), 1);
    equal("marker", observed, marker);
}

static void error_case(const char *name, int fd, long length)
{
    errno = 0;
    int result = ftruncate(fd, length);
    int error = errno;
    printf("ERROR %s %d %d\n", name, result, error);
    equal("error-result", result, -1);
}

int main(int argc, char **argv)
{
    if (argc != 2) return 2;
    int fd = open(argv[1], O_RDWR | O_CREAT | O_EXCL, 0600);
    if (fd < 0) return 3;
    resize_case(fd, 5, 5, 0);
    resize_case(fd, 8, 6, 3);
    resize_case(fd, 8, 2, 3);
    resize_case(fd, 8, 11, 8);
    resize_case(fd, 8, 11, 20);
    resize_case(fd, 8, 25, 20);
    resize_case(fd, 2147483650L, 2147483650L, 2147483649L);
    resize_case(fd, 2147483649L, 2147483650L, 2147483651L);
    equal("reset-length", ftruncate(fd, 8), 0);
    equal("reset-position", lseek(fd, 6, SEEK_SET), 6);
    error_case("negative", fd, -1);
    error_case("minimum", fd, (-9223372036854775807L - 1));
    error_case("maximum-length", fd, 9223372036854775807L);
    equal("error-position", lseek(fd, 0, SEEK_CUR), 6);
    struct stat status;
    equal("error-stat", fstat(fd, &status), 0);
    equal("error-length", status.st_size, 8);
    equal("truncate-after-errors", ftruncate(fd, 0), 0);
    equal("preserved-position-after-errors", lseek(fd, 0, SEEK_CUR), 6);
    int ro = open(argv[1], O_RDONLY);
    if (ro < 0) return 4;
    error_case("readonly", ro, 0);
    equal("close-readonly", close(ro), 0);
    int descriptors[2];
    equal("pipe", pipe(descriptors), 0);
    error_case("pipe-read", descriptors[0], 0);
    error_case("pipe-write", descriptors[1], 0);
    equal("close-pipe-read", close(descriptors[0]), 0);
    equal("close-pipe-write", close(descriptors[1]), 0);
    int sock = socket(AF_UNIX, SOCK_STREAM, 0);
    if (sock < 0) return 5;
    error_case("socket", sock, 0);
    equal("close-socket", close(sock), 0);
    equal("close", close(fd), 0);
    error_case("closed", fd, 0);
    error_case("invalid", -1, 0);
    error_case("invalid-negative", -1, -1);
    equal("unlink", unlink(argv[1]), 0);
    printf("SUMMARY checks=%d failures=%d\n", checks, failures);
    return failures ? 1 : 0;
}
