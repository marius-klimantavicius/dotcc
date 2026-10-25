/* Execute the actual adapted upstream argv.c body, with a minimal guest-memory
   host. This checks Linux zero RDX and preservation of APE's program string. */
#define _DEFAULT_SOURCE 1
#include <assert.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
typedef uint8_t u8;
typedef int64_t i64;
struct Elf { i64 at_entry, at_base, at_phdr, at_phent, at_phnum; };
struct System { struct Elf elf; bool iscosmo; };
struct Machine { struct System *system; u8 sp[8], dx[8], di[8], stack[8192]; };
#define unassert assert
#define AT_UID_LINUX 11
#define AT_EUID_LINUX 12
#define AT_GID_LINUX 13
#define AT_EGID_LINUX 14
#define AT_SECURE_LINUX 23
#define AT_PAGESZ_LINUX 6
#define AT_CLKTCK_LINUX 17
#define AT_RANDOM_LINUX 25
#define AT_EXECFN_LINUX 31
#define AT_PHDR_LINUX 3
#define AT_PHENT_LINUX 4
#define AT_PHNUM_LINUX 5
#define AT_ENTRY_LINUX 9
#define AT_BASE_LINUX 7
static long FLAG_pagesize = 4096;
static i64 Read64(const void *p) { i64 n; memcpy(&n, p, 8); return n; }
#define Get64 Read64
static void Write64(void *p, i64 n) { memcpy(p, &n, 8); }
#define Put64 Write64
static bool HasLinearMapping(void) { return false; }
static bool IsProcessTainted(void) { return false; }
static int CopyToUser(struct Machine *m, i64 p, const void *s, size_t n) {
  assert(p >= 0 && (uint64_t)p + n <= sizeof(m->stack));
  memcpy(m->stack + p, s, n); return 0;
}
#include "adapted-argv.inc"
int main(void) {
  for (int kind = 0; kind != 3; ++kind) {
    struct System system = {.iscosmo = kind == 1, .elf.at_base = -1,
                            .elf.at_phnum = kind == 2 ? 0 : 1};
    struct Machine m = {.system = &system};
    char *argv[] = {"/bin/probe", "argument", 0}, *env[] = {"A=B", 0};
    u8 random[16] = {0};
    Write64(m.sp, sizeof(m.stack));
    LoadArgv(&m, argv[0], argv[0], argv, env, random);
    assert(Read64(m.di) == 0 && !(Read64(m.sp) & 15));
    assert(Read64(m.stack + Read64(m.sp)) == 2);
    if (kind) assert(!strcmp((char *)m.stack + Read64(m.dx), argv[0]));
    else assert(Read64(m.dx) == 0);
  }
  puts("entry-register Linux=zero APE=program-string flat=program-string OK");
}
