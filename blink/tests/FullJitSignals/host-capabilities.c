/* Actual host evidence only: never substitute a simulated mask for this row. */
#include <cpuid.h>
#include <stdint.h>
#include <stdio.h>

int main(void) {
  unsigned a, b, c, d;
  __cpuid_count(1, 0, a, b, c, d);
  unsigned ecx1 = c, edx1 = d;
  uint64_t xcr0 = 0;
  int read_xcr0 = (ecx1 & (1u << 27)) != 0;
  if (read_xcr0) {
    __asm__ volatile("xgetbv" : "=a"(a), "=d"(d) : "c"(0));
    xcr0 = (uint64_t)d << 32 | a;
  }
  __cpuid_count(7, 0, a, b, c, d);
  printf("{\"evidence\":\"actual-linux-x64-host\",\"simulated\":false,"
         "\"leaf1_ecx\":%u,\"leaf1_edx\":%u,\"leaf7_ebx\":%u,"
         "\"leaf7_ecx\":%u,\"xgetbv_executed\":%s,\"xcr0\":%llu}\n",
         ecx1, edx1, b, c, read_xcr0 ? "true" : "false",
         (unsigned long long)xcr0);
  return 0;
}
