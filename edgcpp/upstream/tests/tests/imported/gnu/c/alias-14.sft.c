//type: rp
//options: 
# 0 "./alias-14.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./alias-14.c"


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
# 4 "./alias-14.c" 2

# 4 "./alias-14.c"
void *a;
int *b;
struct c {void * a;} c;
struct d {short * a;} d;

int *ip= (int *)(size_t)2;
int **ipp = &ip;

int
main()
{
  float **ptr;
  void **uptr;
  int* const* cipp = (int* const*)ipp;

  asm ("":"=r"(ptr):"0"(&a));
  a=
# 20 "./alias-14.c" 3 4
   ((void *)0)
# 20 "./alias-14.c"
       ;
  *ptr=(float*)(size_t)1;
  if (!a)
    __builtin_abort ();
  a=
# 24 "./alias-14.c" 3 4
   ((void *)0)
# 24 "./alias-14.c"
       ;
  if (*ptr)
    __builtin_abort ();

  asm ("":"=r"(uptr):"0"(&b));
  b=
# 29 "./alias-14.c" 3 4
   ((void *)0)
# 29 "./alias-14.c"
       ;
  *uptr=(void*)(size_t)1;
  if (!b)
    __builtin_abort ();
  b=
# 33 "./alias-14.c" 3 4
   ((void *)0)
# 33 "./alias-14.c"
       ;
  if (*uptr)
    __builtin_abort ();


  asm ("":"=r"(ptr):"0"(&b));
  b=
# 39 "./alias-14.c" 3 4
   ((void *)0)
# 39 "./alias-14.c"
       ;
  *ptr=(float*)(size_t)1;
  if (b)
    __builtin_abort ();


  asm ("":"=r"(ptr):"0"(&c));
  c.a=
# 46 "./alias-14.c" 3 4
     ((void *)0)
# 46 "./alias-14.c"
         ;
  *ptr=(float*)(size_t)1;
  if (!c.a)
    __builtin_abort ();
  c.a=
# 50 "./alias-14.c" 3 4
     ((void *)0)
# 50 "./alias-14.c"
         ;
  if (*ptr)
    __builtin_abort ();

  asm ("":"=r"(uptr):"0"(&d));
  d.a=
# 55 "./alias-14.c" 3 4
     ((void *)0)
# 55 "./alias-14.c"
         ;
  *uptr=(void*)(size_t)1;
  if (!d.a)
    __builtin_abort ();
  d.a=
# 59 "./alias-14.c" 3 4
     ((void *)0)
# 59 "./alias-14.c"
         ;
  if (*uptr)
    __builtin_abort ();

  if ((void *)*cipp != (void*)(size_t)2)
    __builtin_abort ();
  *ipp = 
# 65 "./alias-14.c" 3 4
        ((void *)0)
# 65 "./alias-14.c"
            ;
  if (*cipp)
    __builtin_abort ();

  return 0;
}
