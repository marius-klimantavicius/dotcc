//type: fp
//options: --c89 --strict_gnu
# 0 "./inline-23.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./inline-23.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 6 "./inline-23.c" 2


# 7 "./inline-23.c"
typedef long int my_intptr_t;

static inline __attribute__((always_inline)) void __check_printsym_format(const
char *fmt, ...)
{
}
static inline __attribute__((always_inline)) void print_symbol(const char *fmt,
my_intptr_t addr)
{
 __check_printsym_format(fmt, "");
}
void do_initcalls(void **call)
{
   print_symbol(": %s()", (my_intptr_t) *call);
}
