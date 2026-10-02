//type: fp
//options: 
# 0 "./debug/ctf/ctf-bitfields-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./debug/ctf/ctf-bitfields-3.c"
# 11 "./debug/ctf/ctf-bitfields-3.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 12 "./debug/ctf/ctf-bitfields-3.c" 2

struct open_file {
  bool mmapped:1;
  bool released:1;
} of;
