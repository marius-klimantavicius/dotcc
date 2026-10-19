/* Independent native/translated FIFO invariants. Identities are compared only
 * within this process: managed queue IDs are not host kernel inode numbers. */
#include <errno.h>
#include <fcntl.h>
#include <poll.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <sys/stat.h>
#include <time.h>
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
static void same_fifo(const struct stat *a, const struct stat *b)
{
    equal("same-device", a->st_dev == b->st_dev, 1);
    equal("same-inode", a->st_ino == b->st_ino, 1);
    equal("same-mode", a->st_mode, b->st_mode);
    equal("same-links", a->st_nlink, b->st_nlink);
    equal("same-uid", a->st_uid, b->st_uid);
    equal("same-gid", a->st_gid, b->st_gid);
    equal("same-size", a->st_size, b->st_size);
    equal("same-access", a->st_atime, b->st_atime);
    equal("same-modify", a->st_mtime, b->st_mtime);
    equal("same-change", a->st_ctime, b->st_ctime);
    equal("same-rdev", a->st_rdev == b->st_rdev, 1);
    equal("same-blocksize", a->st_blksize, b->st_blksize);
    equal("same-blocks", a->st_blocks, b->st_blocks);
}
static int metadata(int fd, struct stat *result)
{
    errno = 123;
    int rc = fstat(fd, result);
    equal("fstat", rc, 0);
    if (rc != 0) return 0;
    equal("success-errno", errno, 123);
    equal("fifo-type", S_ISFIFO(result->st_mode) != 0, 1);
    equal("owner-mode", result->st_mode & 07777, 0600);
    equal("links", result->st_nlink, 1);
    equal("uid", result->st_uid, geteuid());
    equal("gid", result->st_gid, getegid());
    equal("size-is-not-queued-count", result->st_size, 0);
    equal("rdev", result->st_rdev == 0, 1);
    equal("blocks", result->st_blocks, 0);
    equal("preferred-transfer-positive", result->st_blksize > 0, 1);
    return 1;
}
int main(void)
{
    int fd[2], other[2];
    equal("pipe", pipe(fd), 0);
    equal("other-pipe", pipe(other), 0);
    for (int end = 0; end < 2; end++) {
        equal("initial-descriptor-flags", fcntl(fd[end], F_GETFD), 0);
        equal("set-noninheritable", fcntl(fd[end], F_SETFD, FD_CLOEXEC), 0);
        equal("noninheritable-roundtrip", fcntl(fd[end], F_GETFD), FD_CLOEXEC);
    }
    struct stat initial, writer, second, queued, drained, after_close;
    time_t before = time(NULL);
    if (!metadata(fd[0], &initial) || !metadata(fd[1], &writer) || !metadata(other[0], &second))
        return 1;
    same_fifo(&initial, &writer);
    equal("distinct-pipe", initial.st_dev != second.st_dev || initial.st_ino != second.st_ino, 1);
    equal("timestamp-real", initial.st_ctime > 0 && initial.st_ctime <= before, 1);
    equal("creation-access-modify", initial.st_atime == initial.st_mtime, 1);
    equal("creation-modify-change", initial.st_mtime == initial.st_ctime, 1);
    equal("close-other-read", close(other[0]), 0);
    equal("close-other-write", close(other[1]), 0);
    struct stat *null_metadata = 0;
    errno = 0;
    equal("valid-fd-null", fstat(fd[0], null_metadata), -1);
    equal("null-errno", errno, EFAULT);
    errno = 0;
    equal("invalid-fd", fstat(-1, &second), -1);
    equal("invalid-errno", errno, EBADF);
    errno = 0;
    equal("ftruncate-pipe", ftruncate(fd[1], 1), -1);
    equal("truncate-errno", errno, EINVAL);
    errno = 0;
    equal("seek-pipe", lseek(fd[0], 0, SEEK_CUR), -1);
    equal("seek-errno", errno, ESPIPE);
    equal("nonblock", fcntl(fd[0], F_SETFL, O_NONBLOCK), 0);
    unsigned char byte = 0, marker = 0x5a;
    errno = 0;
    equal("empty-read", read(fd[0], &byte, 1), -1);
    equal("empty-errno", errno, EAGAIN);
    struct pollfd ready = { fd[0], POLLIN, 0 };
    equal("empty-poll", poll(&ready, 1, 0), 0);
    sleep(2); /* distinguish actual operation timestamps from creation */
    equal("write", write(fd[1], &marker, 1), 1);
    if (!metadata(fd[0], &queued) || !metadata(fd[1], &writer)) return 1;
    same_fifo(&queued, &writer);
    equal("queue-keeps-identity", queued.st_ino == initial.st_ino && queued.st_dev == initial.st_dev, 1);
    equal("readable-poll", poll(&ready, 1, 0), 1);
    equal("poll-in", (ready.revents & POLLIN) != 0, 1);
    equal("read", read(fd[0], &byte, 1), 1);
    equal("data", byte, marker);
    if (!metadata(fd[0], &drained)) return 1;
    equal("write-keeps-access", queued.st_atime, initial.st_atime);
    equal("write-updates-modify", queued.st_mtime > initial.st_mtime, 1);
    equal("write-updates-change", queued.st_ctime > initial.st_ctime, 1);
    equal("read-updates-access", drained.st_atime > queued.st_atime, 1);
    equal("read-keeps-modify", drained.st_mtime, queued.st_mtime);
    equal("read-keeps-change", drained.st_ctime, queued.st_ctime);
    printf("TIMESTAMPS write_access_changed=%d write_modify_changed=%d write_change_changed=%d read_access_changed=%d read_modify_changed=%d read_change_changed=%d\n",
           queued.st_atime != initial.st_atime, queued.st_mtime != initial.st_mtime,
           queued.st_ctime != initial.st_ctime, drained.st_atime != queued.st_atime,
           drained.st_mtime != queued.st_mtime, drained.st_ctime != queued.st_ctime);
    equal("close-writer", close(fd[1]), 0);
    if (!metadata(fd[0], &after_close)) return 1;
    same_fifo(&drained, &after_close);
    equal("eof-poll", poll(&ready, 1, 0), 1);
    equal("poll-hup", (ready.revents & POLLHUP) != 0, 1);
    equal("eof", read(fd[0], &byte, 1), 0);
    errno = 0;
    equal("closed-writer-fstat", fstat(fd[1], &second), -1);
    equal("closed-writer-errno", errno, EBADF);
    equal("close-reader", close(fd[0]), 0);
    errno = 0;
    equal("closed-reader-fstat", fstat(fd[0], &second), -1);
    equal("closed-reader-errno", errno, EBADF);
    printf("SUMMARY checks=%d failures=%d\n", checks, failures);
    return failures ? 1 : 0;
}
