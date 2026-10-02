//type: rp
//options: 
# 0 "./abi/pr60336-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr60336-2.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./abi/pr60336-2.C" 2


# 6 "./abi/pr60336-2.C"
struct dummy { };

void
test (struct dummy a, int m, ...)
{
  va_list va_arglist;
  int i;
  int count = 0;

  if (m == 0)
    count++;
  
# 17 "./abi/pr60336-2.C" 3 4
 __builtin_va_start(
# 17 "./abi/pr60336-2.C"
 va_arglist
# 17 "./abi/pr60336-2.C" 3 4
 ,
# 17 "./abi/pr60336-2.C"
 m
# 17 "./abi/pr60336-2.C" 3 4
 )
# 17 "./abi/pr60336-2.C"
                         ;
  i = 
# 18 "./abi/pr60336-2.C" 3 4
     __builtin_va_arg(
# 18 "./abi/pr60336-2.C"
     va_arglist
# 18 "./abi/pr60336-2.C" 3 4
     ,
# 18 "./abi/pr60336-2.C"
     int
# 18 "./abi/pr60336-2.C" 3 4
     )
# 18 "./abi/pr60336-2.C"
                             ;
  if (i == 1)
    count++;
  i = 
# 21 "./abi/pr60336-2.C" 3 4
     __builtin_va_arg(
# 21 "./abi/pr60336-2.C"
     va_arglist
# 21 "./abi/pr60336-2.C" 3 4
     ,
# 21 "./abi/pr60336-2.C"
     int
# 21 "./abi/pr60336-2.C" 3 4
     )
# 21 "./abi/pr60336-2.C"
                             ;
  if (i == 2)
  i = 
# 23 "./abi/pr60336-2.C" 3 4
     __builtin_va_arg(
# 23 "./abi/pr60336-2.C"
     va_arglist
# 23 "./abi/pr60336-2.C" 3 4
     ,
# 23 "./abi/pr60336-2.C"
     int
# 23 "./abi/pr60336-2.C" 3 4
     )
# 23 "./abi/pr60336-2.C"
                             ;
    count++;
  if (i == 3)
    count++;
  i = 
# 27 "./abi/pr60336-2.C" 3 4
     __builtin_va_arg(
# 27 "./abi/pr60336-2.C"
     va_arglist
# 27 "./abi/pr60336-2.C" 3 4
     ,
# 27 "./abi/pr60336-2.C"
     int
# 27 "./abi/pr60336-2.C" 3 4
     )
# 27 "./abi/pr60336-2.C"
                             ;
  if (i == 4)
    count++;
  i = 
# 30 "./abi/pr60336-2.C" 3 4
     __builtin_va_arg(
# 30 "./abi/pr60336-2.C"
     va_arglist
# 30 "./abi/pr60336-2.C" 3 4
     ,
# 30 "./abi/pr60336-2.C"
     int
# 30 "./abi/pr60336-2.C" 3 4
     )
# 30 "./abi/pr60336-2.C"
                             ;
  if (i == 5)
    count++;
  i = 
# 33 "./abi/pr60336-2.C" 3 4
     __builtin_va_arg(
# 33 "./abi/pr60336-2.C"
     va_arglist
# 33 "./abi/pr60336-2.C" 3 4
     ,
# 33 "./abi/pr60336-2.C"
     int
# 33 "./abi/pr60336-2.C" 3 4
     )
# 33 "./abi/pr60336-2.C"
                             ;
  if (i == 6)
    count++;
  
# 36 "./abi/pr60336-2.C" 3 4
 __builtin_va_end(
# 36 "./abi/pr60336-2.C"
 va_arglist
# 36 "./abi/pr60336-2.C" 3 4
 )
# 36 "./abi/pr60336-2.C"
                    ;
  if (count != 7)
    __builtin_abort ();
}

struct dummy a0;

int
main ()
{
  test (a0, 0, 1, 2, 3, 4, 5, 6);
  return 0;
}
