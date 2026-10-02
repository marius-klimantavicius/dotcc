//type: rp
//options: --c23
# 0 "./c2x-complit-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2x-complit-2.c"




# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 6 "./c2x-complit-2.c" 2


# 7 "./c2x-complit-2.c"
extern void abort (void);
extern void exit (int);


int *ps = &(static int) { 1 };
size_t ss = sizeof (static int) { 1 };
int *psa = (static int [3]) { 1, 2, 3 };

int
main ()
{
  if (ps[0] != 1)
    abort ();
  if (ss != sizeof (int))
    abort ();
  if (psa[0] != 1 || psa[1] != 2 || psa[2] != 3)
    abort ();
  if ((register int) { 3 } != 3)
    abort ();



  int i = 0;
 lab:
  int *p = &(static int) { 0 };
  if (*p != i)
    abort ();
  i++;
  *p = i;
  if (i < 5)
    goto lab;
  i = 0;
 lab2:
  int *p2 = &(int) { 0 };
  if (*p2 != 0)
    abort ();
  i++;
  *p2 = i;
  if (i < 5)
    goto lab2;
  exit (0);
}
