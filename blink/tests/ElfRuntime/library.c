__thread volatile int elf_dso_tls = 11;
__thread volatile int elf_dso_tls_bss;
int elf_dso_data = 37;
static int *volatile relocated = &elf_dso_data;
static volatile unsigned char zeroed[32768];
int elf_dso_touch(void) {
  if (*relocated != 37 || elf_dso_tls != 11 || elf_dso_tls_bss) return -1;
  for (unsigned i = 0; i < sizeof(zeroed); ++i) if (zeroed[i]) return -2;
  elf_dso_tls = 13; elf_dso_tls_bss = 10; *relocated = 38;
  for (unsigned i = 0; i < sizeof(zeroed); ++i) zeroed[i] = (unsigned char)(i + 1);
  for (unsigned i = 0; i < sizeof(zeroed); ++i) if (zeroed[i] != (unsigned char)(i + 1)) return -3;
  return elf_dso_tls + elf_dso_tls_bss + elf_dso_data;
}
