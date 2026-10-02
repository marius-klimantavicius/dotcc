//type: fn
//options: 
# 0 "./atomic-invalid.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic-invalid.c"





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
# 7 "./atomic-invalid.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 8 "./atomic-invalid.c" 2


# 9 "./atomic-invalid.c"
int i, e, b;
size_t s;
bool x;

int
main ()
{
  __atomic_compare_exchange_n (&i, &e, 1, 0, 0, 5);
  __atomic_compare_exchange_n (&i, &e, 1, 0, 5, 3);
  __atomic_compare_exchange_n (&i, &e, 1, 1, 5, 4);

  __atomic_load_n (&i, 3);
  __atomic_load_n (&i, 4);

  __atomic_store_n (&i, 1, 2);
  __atomic_store_n (&i, 1, 1);
  __atomic_store_n (&i, 1, 4);

  i = __atomic_always_lock_free (s, 
# 27 "./atomic-invalid.c" 3 4
                                   ((void *)0)
# 27 "./atomic-invalid.c"
                                       );

  __atomic_load_n (&i, 44);

  __atomic_clear (&x, 1);
  __atomic_clear (&x, 2);

  __atomic_clear (&x, 4);

}
