//type: rp
//options: 
# 0 "./other/stdarg1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./other/stdarg1.C"




# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./other/stdarg1.C" 2


# 7 "./other/stdarg1.C"
extern "C" void abort (void);

void baz (va_list list)
{
  if (
# 11 "./other/stdarg1.C" 3 4
     __builtin_va_arg(
# 11 "./other/stdarg1.C"
     list
# 11 "./other/stdarg1.C" 3 4
     ,
# 11 "./other/stdarg1.C"
     long
# 11 "./other/stdarg1.C" 3 4
     ) 
# 11 "./other/stdarg1.C"
                         != 3)
    abort ();
}

void foo (long p1, long, long p2, ...)
{
  va_list list;
  
# 18 "./other/stdarg1.C" 3 4
 __builtin_va_start(
# 18 "./other/stdarg1.C"
 list
# 18 "./other/stdarg1.C" 3 4
 ,
# 18 "./other/stdarg1.C"
 p2
# 18 "./other/stdarg1.C" 3 4
 )
# 18 "./other/stdarg1.C"
                    ;
  baz (list);
  
# 20 "./other/stdarg1.C" 3 4
 __builtin_va_end(
# 20 "./other/stdarg1.C"
 list
# 20 "./other/stdarg1.C" 3 4
 )
# 20 "./other/stdarg1.C"
              ;
}

int main ()
{
  foo (0, 1, 2, (long)3);
  return 0;
}
