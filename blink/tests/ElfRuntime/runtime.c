#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static __thread volatile int tls = 7;
static __thread volatile int tls_bss;
static volatile int initialized = 42;
static volatile int *volatile relocated = &initialized;
static volatile unsigned char bss[16384];
#ifdef DYNAMIC_FIXTURE
extern int elf_dso_touch(void);
extern int elf_dso_data;
#endif
static void shutdown_witness(void) { puts("elf-atexit OK"); }
__attribute__((destructor)) static void destructor_witness(void) { puts("elf-destructor OK"); }

int main(int argc, char **argv) {
  if (argc != 2 || strcmp(argv[1], "fixture-argument") || tls != 7 || tls_bss ||
      *relocated != 42 || relocated != &initialized) return 41;
  for (size_t i = 0; i < sizeof(bss); ++i) if (bss[i]) return 42;
  tls += 6; tls_bss = 17; *relocated += 1;
  for (size_t i = 0; i < sizeof(bss); ++i) bss[i] = (unsigned char)(i * 7 + 3);
  for (size_t i = 0; i < sizeof(bss); ++i) if (bss[i] != (unsigned char)(i * 7 + 3)) return 43;
  if (tls != 13 || tls_bss != 17 || initialized != 43) return 44;
  int dso = 0;
#ifdef DYNAMIC_FIXTURE
  if (elf_dso_data != 37 || elf_dso_touch() != 61 || elf_dso_data != 38) return 45;
  dso = 61;
#endif
  char input[6] = {0};
  FILE *file = fopen("input.txt", "rb");
  if (!file || fread(input, 1, 5, file) != 5 || fclose(file) || strcmp(input, "input")) return 46;
  file = fopen("output.txt", "wb");
  if (!file || fwrite("file-write OK\n", 1, 14, file) != 14 || fclose(file)) return 47;
  if (atexit(shutdown_witness)) return 48;
  printf("elf-runtime tls=%d tls-bss=%d data=%d bss=OK file=%s dso=%d\n", tls, tls_bss, initialized, input, dso);
  fputs("elf-stderr OK\n", stderr);
  return 0; /* Real glibc exit/destructors/rtld_fini must finish normally. */
}
