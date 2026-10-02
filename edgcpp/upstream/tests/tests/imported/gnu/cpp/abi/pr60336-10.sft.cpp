//type: rp
//options: 
# 0 "./abi/pr60336-10.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr60336-10.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./abi/pr60336-10.C" 2


# 6 "./abi/pr60336-10.C"
struct dummy0 { };
struct dummy1 { };
struct dummy : dummy0, dummy1 { };

void
test (struct dummy a, int m, ...)
{
  va_list va_arglist;
  int i;
  int count = 0;

  if (m == 0)
    count++;
  
# 19 "./abi/pr60336-10.C" 3 4
 __builtin_va_start(
# 19 "./abi/pr60336-10.C"
 va_arglist
# 19 "./abi/pr60336-10.C" 3 4
 ,
# 19 "./abi/pr60336-10.C"
 m
# 19 "./abi/pr60336-10.C" 3 4
 )
# 19 "./abi/pr60336-10.C"
                         ;
  i = 
# 20 "./abi/pr60336-10.C" 3 4
     __builtin_va_arg(
# 20 "./abi/pr60336-10.C"
     va_arglist
# 20 "./abi/pr60336-10.C" 3 4
     ,
# 20 "./abi/pr60336-10.C"
     int
# 20 "./abi/pr60336-10.C" 3 4
     )
# 20 "./abi/pr60336-10.C"
                             ;
  if (i == 1)
    count++;
  i = 
# 23 "./abi/pr60336-10.C" 3 4
     __builtin_va_arg(
# 23 "./abi/pr60336-10.C"
     va_arglist
# 23 "./abi/pr60336-10.C" 3 4
     ,
# 23 "./abi/pr60336-10.C"
     int
# 23 "./abi/pr60336-10.C" 3 4
     )
# 23 "./abi/pr60336-10.C"
                             ;
  if (i == 2)
  i = 
# 25 "./abi/pr60336-10.C" 3 4
     __builtin_va_arg(
# 25 "./abi/pr60336-10.C"
     va_arglist
# 25 "./abi/pr60336-10.C" 3 4
     ,
# 25 "./abi/pr60336-10.C"
     int
# 25 "./abi/pr60336-10.C" 3 4
     )
# 25 "./abi/pr60336-10.C"
                             ;
    count++;
  if (i == 3)
    count++;
  i = 
# 29 "./abi/pr60336-10.C" 3 4
     __builtin_va_arg(
# 29 "./abi/pr60336-10.C"
     va_arglist
# 29 "./abi/pr60336-10.C" 3 4
     ,
# 29 "./abi/pr60336-10.C"
     int
# 29 "./abi/pr60336-10.C" 3 4
     )
# 29 "./abi/pr60336-10.C"
                             ;
  if (i == 4)
    count++;
  i = 
# 32 "./abi/pr60336-10.C" 3 4
     __builtin_va_arg(
# 32 "./abi/pr60336-10.C"
     va_arglist
# 32 "./abi/pr60336-10.C" 3 4
     ,
# 32 "./abi/pr60336-10.C"
     int
# 32 "./abi/pr60336-10.C" 3 4
     )
# 32 "./abi/pr60336-10.C"
                             ;
  if (i == 5)
    count++;
  i = 
# 35 "./abi/pr60336-10.C" 3 4
     __builtin_va_arg(
# 35 "./abi/pr60336-10.C"
     va_arglist
# 35 "./abi/pr60336-10.C" 3 4
     ,
# 35 "./abi/pr60336-10.C"
     int
# 35 "./abi/pr60336-10.C" 3 4
     )
# 35 "./abi/pr60336-10.C"
                             ;
  if (i == 6)
    count++;
  
# 38 "./abi/pr60336-10.C" 3 4
 __builtin_va_end(
# 38 "./abi/pr60336-10.C"
 va_arglist
# 38 "./abi/pr60336-10.C" 3 4
 )
# 38 "./abi/pr60336-10.C"
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
