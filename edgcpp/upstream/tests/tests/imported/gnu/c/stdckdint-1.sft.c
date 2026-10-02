//type: rp
//options: --c23
# 0 "./stdckdint-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./stdckdint-1.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdckdint.h" 1 3 4
# 6 "./stdckdint-1.c" 2





extern void abort (void);

int
main ()
{
  unsigned int a;
  if (
# 17 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_add_overflow (
# 17 "./stdckdint-1.c"
     1
# 17 "./stdckdint-1.c" 3 4
     , 
# 17 "./stdckdint-1.c"
     2
# 17 "./stdckdint-1.c" 3 4
     , 
# 17 "./stdckdint-1.c"
     &a
# 17 "./stdckdint-1.c" 3 4
     )) 
# 17 "./stdckdint-1.c"
                        || a != 3)
    abort ();
  if (
# 19 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_add_overflow (
# 19 "./stdckdint-1.c"
     ~2U
# 19 "./stdckdint-1.c" 3 4
     , 
# 19 "./stdckdint-1.c"
     2
# 19 "./stdckdint-1.c" 3 4
     , 
# 19 "./stdckdint-1.c"
     &a
# 19 "./stdckdint-1.c" 3 4
     )) 
# 19 "./stdckdint-1.c"
                          || a != ~0U)
    abort ();
  if (!
# 21 "./stdckdint-1.c" 3 4
      ((_Bool) __builtin_add_overflow (
# 21 "./stdckdint-1.c"
      ~2U
# 21 "./stdckdint-1.c" 3 4
      , 
# 21 "./stdckdint-1.c"
      4
# 21 "./stdckdint-1.c" 3 4
      , 
# 21 "./stdckdint-1.c"
      &a
# 21 "./stdckdint-1.c" 3 4
      )) 
# 21 "./stdckdint-1.c"
                           || a != 1)
    abort ();
  if (
# 23 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_sub_overflow (
# 23 "./stdckdint-1.c"
     42
# 23 "./stdckdint-1.c" 3 4
     , 
# 23 "./stdckdint-1.c"
     2
# 23 "./stdckdint-1.c" 3 4
     , 
# 23 "./stdckdint-1.c"
     &a
# 23 "./stdckdint-1.c" 3 4
     )) 
# 23 "./stdckdint-1.c"
                         || a != 40)
    abort ();
  if (!
# 25 "./stdckdint-1.c" 3 4
      ((_Bool) __builtin_sub_overflow (
# 25 "./stdckdint-1.c"
      11
# 25 "./stdckdint-1.c" 3 4
      , 
# 25 "./stdckdint-1.c"
      ~0ULL
# 25 "./stdckdint-1.c" 3 4
      , 
# 25 "./stdckdint-1.c"
      &a
# 25 "./stdckdint-1.c" 3 4
      )) 
# 25 "./stdckdint-1.c"
                              || a != 12)
    abort ();
  if (
# 27 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_mul_overflow (
# 27 "./stdckdint-1.c"
     42
# 27 "./stdckdint-1.c" 3 4
     , 
# 27 "./stdckdint-1.c"
     16U
# 27 "./stdckdint-1.c" 3 4
     , 
# 27 "./stdckdint-1.c"
     &a
# 27 "./stdckdint-1.c" 3 4
     )) 
# 27 "./stdckdint-1.c"
                           || a != 672)
    abort ();
  if (
# 29 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_mul_overflow (
# 29 "./stdckdint-1.c"
     ~0UL
# 29 "./stdckdint-1.c" 3 4
     , 
# 29 "./stdckdint-1.c"
     0
# 29 "./stdckdint-1.c" 3 4
     , 
# 29 "./stdckdint-1.c"
     &a
# 29 "./stdckdint-1.c" 3 4
     )) 
# 29 "./stdckdint-1.c"
                           || a != 0)
    abort ();
  if (
# 31 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_mul_overflow (
# 31 "./stdckdint-1.c"
     1
# 31 "./stdckdint-1.c" 3 4
     , 
# 31 "./stdckdint-1.c"
     ~0U
# 31 "./stdckdint-1.c" 3 4
     , 
# 31 "./stdckdint-1.c"
     &a
# 31 "./stdckdint-1.c" 3 4
     )) 
# 31 "./stdckdint-1.c"
                          || a != ~0U)
    abort ();
  if (
# 33 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_mul_overflow (
# 33 "./stdckdint-1.c"
     ~0UL
# 33 "./stdckdint-1.c" 3 4
     , 
# 33 "./stdckdint-1.c"
     1
# 33 "./stdckdint-1.c" 3 4
     , 
# 33 "./stdckdint-1.c"
     &a
# 33 "./stdckdint-1.c" 3 4
     )) 
# 33 "./stdckdint-1.c"
                           != (~0UL > ~0U) || a != ~0U)
    abort ();
  static_assert (_Generic (
# 35 "./stdckdint-1.c" 3 4
                          ((_Bool) __builtin_add_overflow (
# 35 "./stdckdint-1.c"
                          1
# 35 "./stdckdint-1.c" 3 4
                          , 
# 35 "./stdckdint-1.c"
                          1
# 35 "./stdckdint-1.c" 3 4
                          , 
# 35 "./stdckdint-1.c"
                          &a
# 35 "./stdckdint-1.c" 3 4
                          ))
# 35 "./stdckdint-1.c"
                                            , bool: 1, default: 0));
  static_assert (_Generic (
# 36 "./stdckdint-1.c" 3 4
                          ((_Bool) __builtin_sub_overflow (
# 36 "./stdckdint-1.c"
                          1
# 36 "./stdckdint-1.c" 3 4
                          , 
# 36 "./stdckdint-1.c"
                          1
# 36 "./stdckdint-1.c" 3 4
                          , 
# 36 "./stdckdint-1.c"
                          &a
# 36 "./stdckdint-1.c" 3 4
                          ))
# 36 "./stdckdint-1.c"
                                            , bool: 1, default: 0));
  static_assert (_Generic (
# 37 "./stdckdint-1.c" 3 4
                          ((_Bool) __builtin_mul_overflow (
# 37 "./stdckdint-1.c"
                          1
# 37 "./stdckdint-1.c" 3 4
                          , 
# 37 "./stdckdint-1.c"
                          1
# 37 "./stdckdint-1.c" 3 4
                          , 
# 37 "./stdckdint-1.c"
                          &a
# 37 "./stdckdint-1.c" 3 4
                          ))
# 37 "./stdckdint-1.c"
                                            , bool: 1, default: 0));
  signed char b;
  if (
# 39 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_add_overflow (
# 39 "./stdckdint-1.c"
     8
# 39 "./stdckdint-1.c" 3 4
     , 
# 39 "./stdckdint-1.c"
     12
# 39 "./stdckdint-1.c" 3 4
     , 
# 39 "./stdckdint-1.c"
     &b
# 39 "./stdckdint-1.c" 3 4
     )) 
# 39 "./stdckdint-1.c"
                         || b != 20)
    abort ();
  if (
# 41 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_sub_overflow (
# 41 "./stdckdint-1.c"
     8UL
# 41 "./stdckdint-1.c" 3 4
     , 
# 41 "./stdckdint-1.c"
     12ULL
# 41 "./stdckdint-1.c" 3 4
     , 
# 41 "./stdckdint-1.c"
     &b
# 41 "./stdckdint-1.c" 3 4
     )) 
# 41 "./stdckdint-1.c"
                              || b != -4)
    abort ();
  if (
# 43 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_mul_overflow (
# 43 "./stdckdint-1.c"
     2
# 43 "./stdckdint-1.c" 3 4
     , 
# 43 "./stdckdint-1.c"
     3
# 43 "./stdckdint-1.c" 3 4
     , 
# 43 "./stdckdint-1.c"
     &b
# 43 "./stdckdint-1.c" 3 4
     )) 
# 43 "./stdckdint-1.c"
                        || b != 6)
    abort ();
  unsigned char c;
  if (
# 46 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_add_overflow (
# 46 "./stdckdint-1.c"
     8
# 46 "./stdckdint-1.c" 3 4
     , 
# 46 "./stdckdint-1.c"
     12
# 46 "./stdckdint-1.c" 3 4
     , 
# 46 "./stdckdint-1.c"
     &c
# 46 "./stdckdint-1.c" 3 4
     )) 
# 46 "./stdckdint-1.c"
                         || c != 20)
    abort ();
  if (
# 48 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_sub_overflow (
# 48 "./stdckdint-1.c"
     8UL
# 48 "./stdckdint-1.c" 3 4
     , 
# 48 "./stdckdint-1.c"
     12ULL
# 48 "./stdckdint-1.c" 3 4
     , 
# 48 "./stdckdint-1.c"
     &c
# 48 "./stdckdint-1.c" 3 4
     )) 
# 48 "./stdckdint-1.c"
                              != (-4ULL > (unsigned char) -4U)
      || c != (unsigned char) -4U)
    abort ();
  if (
# 51 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_mul_overflow (
# 51 "./stdckdint-1.c"
     2
# 51 "./stdckdint-1.c" 3 4
     , 
# 51 "./stdckdint-1.c"
     3
# 51 "./stdckdint-1.c" 3 4
     , 
# 51 "./stdckdint-1.c"
     &c
# 51 "./stdckdint-1.c" 3 4
     )) 
# 51 "./stdckdint-1.c"
                        || c != 6)
    abort ();
  long long d;
  if (
# 54 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_add_overflow (
# 54 "./stdckdint-1.c"
     ~0U
# 54 "./stdckdint-1.c" 3 4
     , 
# 54 "./stdckdint-1.c"
     ~0U
# 54 "./stdckdint-1.c" 3 4
     , 
# 54 "./stdckdint-1.c"
     &d
# 54 "./stdckdint-1.c" 3 4
     )) 
# 54 "./stdckdint-1.c"
                            != (~0U + 1ULL < ~0U)
      || d != (long long) (2 * (unsigned long long) ~0U))
    abort ();
  if (
# 57 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_sub_overflow (
# 57 "./stdckdint-1.c"
     0
# 57 "./stdckdint-1.c" 3 4
     , 
# 57 "./stdckdint-1.c"
     0
# 57 "./stdckdint-1.c" 3 4
     , 
# 57 "./stdckdint-1.c"
     &d
# 57 "./stdckdint-1.c" 3 4
     )) 
# 57 "./stdckdint-1.c"
                        || d != 0)
    abort ();
  if (
# 59 "./stdckdint-1.c" 3 4
     ((_Bool) __builtin_mul_overflow (
# 59 "./stdckdint-1.c"
     16
# 59 "./stdckdint-1.c" 3 4
     , 
# 59 "./stdckdint-1.c"
     1
# 59 "./stdckdint-1.c" 3 4
     , 
# 59 "./stdckdint-1.c"
     &d
# 59 "./stdckdint-1.c" 3 4
     )) 
# 59 "./stdckdint-1.c"
                         || d != 16)
    abort ();
}
