/* Shared libc closure from actual CPython raw diagnostics. The native control
   uses glibc wchar4; emitted mode explicitly checks UTF-16 units instead. */
#define _POSIX_C_SOURCE 200809L
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <stddef.h>
#include <errno.h>
#include <math.h>
#include <float.h>
#include <locale.h>
#include <langinfo.h>
#include <wchar.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <utime.h>
#include <pwd.h>
#include <pthread.h>
#include <stdatomic.h>

static int checks;
#define REQUIRE(x) do { ++checks; if (!(x)) { fprintf(stderr, "host-services assertion %d errno %d\n", __LINE__, errno); exit(1); } } while (0)

static _Atomic int workers_ready;
/* Retain address bits, not pointers to native workers' expired stack records. */
static uintptr_t passwd_addresses[2];
static pthread_mutex_t passwd_gate = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t passwd_condition = PTHREAD_COND_INITIALIZER;

static unsigned long long passwd_text_digest(const char *text, unsigned long long digest) {
    if (text == NULL) return digest ^ 0xff;
    for (; *text; ++text) digest = (digest ^ (unsigned char)*text) * 1099511628211ULL;
    return digest * 1099511628211ULL;
}

static void *passwd_worker(void *argument) {
    int index = (int)(intptr_t)argument;
    struct passwd *entry;
#ifndef DOTCC_EXPECT_UTF16
    /* Native getpwuid itself is not reentrant. Its _r oracle supplies real
       worker-local NSS storage for the stronger shared-runtime contract. */
    struct passwd record; char storage[16384];
    if (getpwuid_r(getuid(), &record, storage, sizeof(storage), &entry) != 0 || entry == NULL) return (void *)(intptr_t)1;
#else
    entry = getpwuid(getuid());
    if (entry == NULL) return (void *)(intptr_t)1;
#endif
    passwd_addresses[index] = (uintptr_t)entry;
    char name[1024];
    if (strlen(entry->pw_name) >= sizeof(name)) return (void *)(intptr_t)2;
    strcpy(name, entry->pw_name);
    errno = EIO + index;
    if (pthread_mutex_lock(&passwd_gate) != 0) return (void *)(intptr_t)3;
    atomic_fetch_add(&workers_ready, 1);
    pthread_cond_broadcast(&passwd_condition);
    while (atomic_load(&workers_ready) != 2)
        if (pthread_cond_wait(&passwd_condition, &passwd_gate) != 0) return (void *)(intptr_t)4;
    pthread_mutex_unlock(&passwd_gate);
    if (strcmp(entry->pw_name, name) != 0 || entry->pw_uid != getuid() || errno != EIO + index) return (void *)(intptr_t)5;
    return NULL;
}

static void math_services(void) {
    REQUIRE(signbit(copysign(0.0, -1.0)));
    REQUIRE(!signbit(copysign(-0.0, 1.0)));
    REQUIRE(copysign(INFINITY, -0.0) == -INFINITY);
    REQUIRE(isnan(copysign(NAN, -1.0)) && signbit(copysign(NAN, -1.0)));
    REQUIRE(signbit(nextafter(0.0, -0.0)));
    REQUIRE(!signbit(nextafter(-0.0, 0.0)));
    REQUIRE(nextafter(0.0, 1.0) == 0x1p-1074);
    REQUIRE(nextafter(1.0, 2.0) == 0x1.0000000000001p0);
    REQUIRE(nextafter(INFINITY, 0.0) == DBL_MAX);
    REQUIRE(isnan(nextafter(NAN, 1.0)));
    errno = 0;
    REQUIRE(nextafter(DBL_MAX, INFINITY) == INFINITY && errno == ERANGE);
    errno = 0;
    REQUIRE(nextafter(0x1p-1074, 0.0) == 0.0 && errno == ERANGE);
    volatile double x = 0x1.0000000000001p0, y = 0x1.ffffffffffffep-1;
    REQUIRE(fma(x, y, -1.0) == -0x1p-104);
    REQUIRE(signbit(fma(-0.0, 2.0, -0.0)));
    REQUIRE(isnan(fma(INFINITY, 0.0, 1.0)));
    REQUIRE(exp2(-1074.0) == 0x1p-1074 && exp2(10.0) == 1024.0);
    REQUIRE(exp2(-INFINITY) == 0.0 && exp2(INFINITY) == INFINITY && isnan(exp2(NAN)));
    REQUIRE(erf(INFINITY) == 1.0 && erf(-INFINITY) == -1.0 && signbit(erf(-0.0)));
    REQUIRE(erfc(INFINITY) == 0.0 && erfc(-INFINITY) == 2.0 && erfc(0.0) == 1.0);
    REQUIRE(isnan(erf(NAN)) && isnan(erfc(NAN)));
    double values[] = { -27, -6, -1, -0.5, -0x1p-40, 0, 0x1p-40, 0.5, 1, 6, 27 };
    for (int i = 0; i < 11; i++) {
        union { double value; uint64_t bits; } a, b, c;
        a.value = erf(values[i]); b.value = erfc(values[i]); c.value = exp2(values[i]);
        printf("math %d %016llx %016llx %016llx\n", i, (unsigned long long)a.bits, (unsigned long long)b.bits, (unsigned long long)c.bits);
    }
}

static void text_services(void) {
    REQUIRE(setlocale(LC_ALL, "C") != NULL);
    REQUIRE(strcmp(nl_langinfo(CODESET), "ANSI_X3.4-1968") == 0);
    int items[] = { ABDAY_1, ABDAY_2, ABDAY_3, ABDAY_4, ABDAY_5, ABDAY_6, ABDAY_7,
        DAY_1, DAY_2, DAY_3, DAY_4, DAY_5, DAY_6, DAY_7,
        ABMON_1, ABMON_2, ABMON_3, ABMON_4, ABMON_5, ABMON_6, ABMON_7, ABMON_8, ABMON_9, ABMON_10, ABMON_11, ABMON_12,
        MON_1, MON_2, MON_3, MON_4, MON_5, MON_6, MON_7, MON_8, MON_9, MON_10, MON_11, MON_12,
        AM_STR, PM_STR, D_T_FMT, D_FMT, T_FMT, T_FMT_AMPM, ERA, ERA_D_FMT, ERA_D_T_FMT, ERA_T_FMT, ALT_DIGITS,
        RADIXCHAR, THOUSEP, YESEXPR, NOEXPR, CRNCYSTR };
    unsigned long long conventions = 14695981039346656037ULL;
    for (size_t i = 0; i < sizeof(items) / sizeof(items[0]); i++) {
        conventions = (conventions ^ (unsigned int)items[i]) * 1099511628211ULL;
        const char *value = nl_langinfo(items[i]);
        conventions = passwd_text_digest(value, conventions);
        printf("langitem %d ", items[i]);
        for (const unsigned char *p = (const unsigned char *)value; *p; ++p) printf("%02x", (unsigned int)*p);
        putchar('\n');
    }
    printf("langinfo %016llx\n", conventions);
    wchar_t wide[16]; char bytes[32];
    errno = EIO;
    REQUIRE(mbstowcs(wide, "ab\0bad", 16) == 2 && wide[2] == 0 && errno == EIO);
    REQUIRE(wcstombs(bytes, wide, 32) == 2 && strcmp(bytes, "ab") == 0);
    errno = 0;
    REQUIRE(mbstowcs(wide, "\xc2\xa3", 16) == (size_t)-1 && errno == EILSEQ);
    REQUIRE(setlocale(LC_ALL, "C.UTF-8") != NULL);
    REQUIRE(strcmp(nl_langinfo(CODESET), "UTF-8") == 0);
    unsigned long long utf8_conventions = 14695981039346656037ULL;
    for (size_t i = 0; i < sizeof(items) / sizeof(items[0]); i++) {
        const char *value = nl_langinfo(items[i]);
        utf8_conventions = (utf8_conventions ^ (unsigned int)items[i]) * 1099511628211ULL;
        utf8_conventions = passwd_text_digest(value, utf8_conventions);
        printf("langutf8item %d ", items[i]);
        for (const unsigned char *p = (const unsigned char *)value; *p; ++p) printf("%02x", (unsigned int)*p);
        putchar('\n');
    }
    REQUIRE(utf8_conventions == conventions);
    printf("langinfo-utf8 %016llx\n", utf8_conventions);
    const char *text = "A\xce\xbb\xf0\x9f\x90\x8d";
#ifdef DOTCC_EXPECT_UTF16
    REQUIRE(sizeof(wchar_t) == 2 && (wchar_t)-1 > 0);
    REQUIRE(mbstowcs(NULL, text, 0) == 4);
    REQUIRE(mbstowcs(wide, text, 16) == 4 && wide[0] == 65 && wide[1] == 0x3bb && wide[2] == 0xd83d && wide[3] == 0xdc0d && wide[4] == 0);
    wide[2] = 0x1234;
    REQUIRE(mbstowcs(wide, text, 3) == 2 && wide[2] == 0x1234);
    REQUIRE(mbstowcs(wide, text, 16) == 4);
#else
    REQUIRE(sizeof(wchar_t) == 4);
    REQUIRE(mbstowcs(NULL, text, 0) == 3);
    REQUIRE(mbstowcs(wide, text, 16) == 3 && wide[2] == 0x1f40d && wide[3] == 0);
#endif
    REQUIRE(wcstombs(NULL, wide, 0) == 7);
    bytes[3] = 0x55;
    REQUIRE(wcstombs(bytes, wide, 3) == 3 && bytes[3] == 0x55);
    REQUIRE(wcstombs(bytes, wide, 32) == 7 && strcmp(bytes, text) == 0);
    bytes[2] = 0x55;
    REQUIRE(wcstombs(bytes, wide, 2) == 1 && bytes[2] == 0x55);
    errno = 0;
    REQUIRE(mbstowcs(wide, "\xf0\x80\x80\x80", 16) == (size_t)-1 && errno == EILSEQ);
    errno = 0;
    REQUIRE(mbstowcs(wide, "\xed\xa0\x80", 16) == (size_t)-1 && errno == EILSEQ);
    errno = 0;
    REQUIRE(mbstowcs(wide, "\xe2\x82", 16) == (size_t)-1 && errno == EILSEQ);
    wide[0] = 0xd800; wide[1] = 0;
    errno = 0;
    REQUIRE(wcstombs(bytes, wide, 32) == (size_t)-1 && errno == EILSEQ);
    REQUIRE(wcstombs(bytes, wide, 0) == 0);
    REQUIRE(mbstowcs(wide, "\xff", 0) == 0);
    REQUIRE(setlocale(LC_ALL, "C") != NULL);
}

static void file_services(void) {
    char path[] = "host-r5-file-XXXXXX";
    int fd = mkstemp(path);
    REQUIRE(fd >= 0);
    REQUIRE(write(fd, "seed", 4) == 4 && lseek(fd, 0, SEEK_SET) == 0);
    FILE *file = fdopen(fd, "w+");
    REQUIRE(file != NULL && fileno(file) == fd); /* w must not truncate */
    REQUIRE(fgetc(file) == 's');
    char buffer[BUFSIZ];
    /* setvbuf must precede I/O; use a freshly opened stream for buffering. */
    REQUIRE(fclose(file) == 0);
    errno = 0;
    REQUIRE(read(fd, buffer, 1) == -1 && errno == EBADF);
    fd = open(path, O_RDWR | O_TRUNC);
    REQUIRE(fd >= 0);
    file = fdopen(fd, "r+"); REQUIRE(file != NULL);
    setbuf(file, buffer);
    REQUIRE(fwrite("hello", 1, 5, file) == 5);
    REQUIRE(ftell(file) == 5 && fflush(file) == 0);
    REQUIRE(fseek(file, 0, SEEK_SET) == 0);
    char readback[16];
    REQUIRE(fread(readback, 1, 5, file) == 5 && memcmp(readback, "hello", 5) == 0);
    REQUIRE(fclose(file) == 0);
    fd = open(path, O_RDONLY);
    REQUIRE(fd >= 0);
    errno = 0;
    REQUIRE(fdopen(fd, "w") == NULL && errno == EINVAL);
    REQUIRE(read(fd, buffer, 5) == 5 && close(fd) == 0); /* failed fdopen keeps ownership */
    fd = open(path, O_RDWR);
    file = fdopen(fd, "a+"); REQUIRE(file != NULL);
    setbuf(file, NULL);
    REQUIRE(fseek(file, 0, SEEK_SET) == 0 && fputc('!', file) == '!');
    REQUIRE(lseek(fd, 0, SEEK_END) == 6); /* unbuffered append reached backing */
    REQUIRE(fclose(file) == 0);
    struct utimbuf stamp = {123456789, 987654321};
    REQUIRE(sizeof(stamp) == 16 && offsetof(struct utimbuf, modtime) == 8);
    REQUIRE(utime(path, &stamp) == 0);
    struct stat st;
    REQUIRE(stat(path, &st) == 0 && st.st_atime == stamp.actime && st.st_mtime == stamp.modtime);
    REQUIRE(utime(path, NULL) == 0);
    const char *linkname = "host-r5-symlink";
    REQUIRE(symlink(path, linkname) == 0);
    errno = 0;
    REQUIRE(open(linkname, O_RDONLY | O_NOFOLLOW) == -1 && errno == ELOOP);
    fd = open(path, O_RDONLY | O_NOFOLLOW); REQUIRE(fd >= 0 && close(fd) == 0);
    fd = open(path, O_RDONLY);
    REQUIRE(fd >= 0);
    size_t page = (size_t)sysconf(_SC_PAGESIZE);
    char *view = mmap(NULL, page, PROT_READ, MAP_PRIVATE, fd, 0);
    REQUIRE(view != MAP_FAILED && close(fd) == 0);
    REQUIRE(memcmp(view, "hello!", 6) == 0 && munmap(view, page) == 0);
    REQUIRE(unlink(linkname) == 0 && unlink(path) == 0);
    errno = 0;
    REQUIRE(utime(path, &stamp) == -1 && errno == ENOENT);
}

static void os_services(void) {
    uid_t uid = getuid(), euid = geteuid();
    gid_t gid = getgid(), egid = getegid();
    REQUIRE(uid != (uid_t)-1 && euid != (uid_t)-1 && gid != (gid_t)-1 && egid != (gid_t)-1);
    printf("identity %u %u %u %u\n", uid, euid, gid, egid);
    REQUIRE(sizeof(struct passwd) == 48 && offsetof(struct passwd, pw_uid) == 16 && offsetof(struct passwd, pw_gid) == 20);
    REQUIRE(offsetof(struct passwd, pw_name) == 0 && offsetof(struct passwd, pw_passwd) == 8);
    REQUIRE(offsetof(struct passwd, pw_gecos) == 24 && offsetof(struct passwd, pw_dir) == 32 && offsetof(struct passwd, pw_shell) == 40);
    struct passwd *pw = getpwuid(getuid());
    REQUIRE(pw != NULL && pw->pw_uid == getuid() && pw->pw_name != NULL && pw->pw_dir != NULL && pw->pw_shell != NULL);
    unsigned long long digest = passwd_text_digest(pw->pw_name, 14695981039346656037ULL);
    digest = passwd_text_digest(pw->pw_passwd, digest);
    digest = passwd_text_digest(pw->pw_gecos, digest);
    digest = passwd_text_digest(pw->pw_dir, digest);
    digest = passwd_text_digest(pw->pw_shell, digest);
    printf("passwd %u %u %016llx\n", pw->pw_uid, pw->pw_gid, digest);
    char name[1024]; REQUIRE(strlen(pw->pw_name) < sizeof(name)); strcpy(name, pw->pw_name);
    REQUIRE(getpwnam(name) != NULL && strcmp(getpwnam(name)->pw_name, name) == 0);
    REQUIRE(getpwnam("dotcc-user-that-does-not-exist-r5") == NULL);
    pthread_t workers[2]; void *result;
    REQUIRE(pthread_create(&workers[0], NULL, passwd_worker, (void *)(intptr_t)0) == 0);
    REQUIRE(pthread_create(&workers[1], NULL, passwd_worker, (void *)(intptr_t)1) == 0);
    REQUIRE(pthread_join(workers[0], &result) == 0 && result == NULL);
    REQUIRE(pthread_join(workers[1], &result) == 0 && result == NULL);
    REQUIRE(passwd_addresses[0] != passwd_addresses[1]);
    REQUIRE(pthread_cond_destroy(&passwd_condition) == 0 && pthread_mutex_destroy(&passwd_gate) == 0);
    struct timespec resolution;
    REQUIRE(clock_getres(CLOCK_MONOTONIC, &resolution) == 0 && resolution.tv_sec == 0 && resolution.tv_nsec > 0 && resolution.tv_nsec <= 1000000);
    REQUIRE(clock_getres(CLOCK_REALTIME, NULL) == 0);
    errno = 0;
    REQUIRE(clock_getres(-123, &resolution) == -1 && errno == EINVAL);
    long page = sysconf(_SC_PAGESIZE);
    REQUIRE(page > 0);
    char *mapping = mmap(NULL, (size_t)page * 3, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    REQUIRE(mapping != MAP_FAILED && (uintptr_t)mapping % (size_t)page == 0);
    REQUIRE(mapping[0] == 0 && mapping[page * 3 - 1] == 0);
    mapping[0] = 42; mapping[page * 2] = 21;
    REQUIRE(munmap(mapping + page, (size_t)page) == 0);
    REQUIRE(mapping[0] == 42 && mapping[page * 2] == 21);
    REQUIRE(munmap(mapping, (size_t)page) == 0 && munmap(mapping + page * 2, (size_t)page) == 0);
    errno = 0;
    REQUIRE(munmap(NULL, 0) == -1 && errno == EINVAL);
    errno = 0;
    REQUIRE(mmap(NULL, 0, PROT_READ, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0) == MAP_FAILED && errno == EINVAL);
#ifdef DOTCC_EXPECT_UTF16
    errno = 0;
    REQUIRE(pause() == -1 && errno == ENOTSUP); /* no product signal install */
#endif
}

int main(void) {
    math_services(); text_services(); file_services(); os_services();
    printf("host-services passed checks=%d\n", checks);
    return 0;
}
