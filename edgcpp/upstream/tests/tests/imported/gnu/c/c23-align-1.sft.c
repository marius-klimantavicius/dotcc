//type: fp
//options: --c23
# 0 "./c23-align-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-align-1.c"




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
# 6 "./c23-align-1.c" 2


# 7 "./c23-align-1.c"
alignas (alignof (max_align_t)) char c;
extern alignas (max_align_t) char c;
extern char c;

extern alignas (max_align_t) short s;
alignas (max_align_t) short s;

alignas (int) int i;
extern int i;

alignas (max_align_t) long l;

alignas (max_align_t) long long ll;

alignas (max_align_t) float f;

alignas (max_align_t) double d;

alignas (max_align_t) _Complex long double cld;

alignas (0) alignas (int) alignas (char) char ca[10];

alignas ((int) alignof (max_align_t) + 0) int x;

enum e { E = alignof (max_align_t) };
alignas (E) int y;

void
func (void)
{
  alignas (max_align_t) long long auto_ll;
}


alignas (0) struct s;
