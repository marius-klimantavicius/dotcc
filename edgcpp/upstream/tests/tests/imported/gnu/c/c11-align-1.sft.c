//type: fp
//options: --c11
# 0 "./c11-align-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-align-1.c"




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
# 6 "./c11-align-1.c" 2


# 7 "./c11-align-1.c"
_Alignas (_Alignof (max_align_t)) char c;
extern _Alignas (max_align_t) char c;
extern char c;

extern _Alignas (max_align_t) short s;
_Alignas (max_align_t) short s;

_Alignas (int) int i;
extern int i;

_Alignas (max_align_t) long l;

_Alignas (max_align_t) long long ll;

_Alignas (max_align_t) float f;

_Alignas (max_align_t) double d;

_Alignas (max_align_t) _Complex long double cld;

_Alignas (0) _Alignas (int) _Alignas (char) char ca[10];

_Alignas ((int) _Alignof (max_align_t) + 0) int x;

enum e { E = _Alignof (max_align_t) };
_Alignas (E) int y;

void
func (void)
{
  _Alignas (max_align_t) long long auto_ll;
}


_Alignas (0) struct s;
