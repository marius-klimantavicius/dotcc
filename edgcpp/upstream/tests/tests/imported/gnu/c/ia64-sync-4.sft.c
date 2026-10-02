//type: fp
//options: 
# 0 "./ia64-sync-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ia64-sync-4.c"
# 9 "./ia64-sync-4.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 10 "./ia64-sync-4.c" 2

static bool
compare_and_swap(long *addr, long old, long new_val)
{
  return __sync_bool_compare_and_swap(addr, old, new_val);
}

void
foo (long *address)
{
  long he_address = *address & ~1;
  while (!compare_and_swap(address, he_address, he_address | 1))
    he_address = *address & ~1;
}
