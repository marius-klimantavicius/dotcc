//type: fp
//options: 
# 0 "./eh/stdarg1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./eh/stdarg1.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./eh/stdarg1.C" 2


# 6 "./eh/stdarg1.C"
int
foo (int a, ...)
{
  va_list ap;
  int r = 0;
  
# 11 "./eh/stdarg1.C" 3 4
 __builtin_va_start(
# 11 "./eh/stdarg1.C"
 ap
# 11 "./eh/stdarg1.C" 3 4
 ,
# 11 "./eh/stdarg1.C"
 a
# 11 "./eh/stdarg1.C" 3 4
 )
# 11 "./eh/stdarg1.C"
                 ;
  try
    {
      if (a == 1)
 throw (ap);
    }
  catch (va_list b)
    {
      r = 
# 19 "./eh/stdarg1.C" 3 4
         __builtin_va_arg(
# 19 "./eh/stdarg1.C"
         b
# 19 "./eh/stdarg1.C" 3 4
         ,
# 19 "./eh/stdarg1.C"
         int
# 19 "./eh/stdarg1.C" 3 4
         )
# 19 "./eh/stdarg1.C"
                        ;
    }
  
# 21 "./eh/stdarg1.C" 3 4
 __builtin_va_end(
# 21 "./eh/stdarg1.C"
 ap
# 21 "./eh/stdarg1.C" 3 4
 )
# 21 "./eh/stdarg1.C"
            ;
  return r;
}

int
main ()
{
  if (foo (0) != 0 || foo (1, 7) != 7)
    __builtin_abort ();
}
