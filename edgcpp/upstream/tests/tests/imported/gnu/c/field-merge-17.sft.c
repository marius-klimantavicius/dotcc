//type: rp
//options: 
# 0 "./field-merge-17.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./field-merge-17.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 7 "./field-merge-17.c" 2


# 8 "./field-merge-17.c"
struct s {
  short a;
  long long b;
  int c;
  long long d;
  short e;
} __attribute__ ((packed, aligned (8)));

struct s p = { 0, 0, 0, 0, 0 };

__attribute__ ((__noinline__, __noipa__, __noclone__))
int fp ()
{
  if (p.a
      || p.b
      || p.c
      || p.d
      || p.e)
    return 1;
  else
    return -1;
}

int main () {

  if (sizeof (long long) == sizeof (short))
    return 0;
  if (fp () > 0)
    __builtin_abort ();
  unsigned char *pc = (unsigned char *)&p;
  for (int i = 0; i < 
# 38 "./field-merge-17.c" 3 4
                     __builtin_offsetof (
# 38 "./field-merge-17.c"
                     struct s
# 38 "./field-merge-17.c" 3 4
                     , 
# 38 "./field-merge-17.c"
                     e
# 38 "./field-merge-17.c" 3 4
                     ) 
# 38 "./field-merge-17.c"
                                            + sizeof (p.e); i++)
    {
      pc[i] = 1;
      if (fp () < 0)
 __builtin_abort ();
      pc[i] = 0;
    }
  return 0;
}
