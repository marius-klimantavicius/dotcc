//type: rp
//options: 
# 0 "./abi/pr60336-11.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr60336-11.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./abi/pr60336-11.C" 2


# 6 "./abi/pr60336-11.C"
struct dummy0
{
  void bar (void);
};
struct dummy1
{
  void foo (void);
};
struct dummy : dummy0, dummy1 { };

void
test (struct dummy a, int m, ...)
{
  va_list va_arglist;
  int i;
  int count = 0;

  if (m == 0)
    count++;
  
# 25 "./abi/pr60336-11.C" 3 4
 __builtin_va_start(
# 25 "./abi/pr60336-11.C"
 va_arglist
# 25 "./abi/pr60336-11.C" 3 4
 ,
# 25 "./abi/pr60336-11.C"
 m
# 25 "./abi/pr60336-11.C" 3 4
 )
# 25 "./abi/pr60336-11.C"
                         ;
  i = 
# 26 "./abi/pr60336-11.C" 3 4
     __builtin_va_arg(
# 26 "./abi/pr60336-11.C"
     va_arglist
# 26 "./abi/pr60336-11.C" 3 4
     ,
# 26 "./abi/pr60336-11.C"
     int
# 26 "./abi/pr60336-11.C" 3 4
     )
# 26 "./abi/pr60336-11.C"
                             ;
  if (i == 1)
    count++;
  i = 
# 29 "./abi/pr60336-11.C" 3 4
     __builtin_va_arg(
# 29 "./abi/pr60336-11.C"
     va_arglist
# 29 "./abi/pr60336-11.C" 3 4
     ,
# 29 "./abi/pr60336-11.C"
     int
# 29 "./abi/pr60336-11.C" 3 4
     )
# 29 "./abi/pr60336-11.C"
                             ;
  if (i == 2)
  i = 
# 31 "./abi/pr60336-11.C" 3 4
     __builtin_va_arg(
# 31 "./abi/pr60336-11.C"
     va_arglist
# 31 "./abi/pr60336-11.C" 3 4
     ,
# 31 "./abi/pr60336-11.C"
     int
# 31 "./abi/pr60336-11.C" 3 4
     )
# 31 "./abi/pr60336-11.C"
                             ;
    count++;
  if (i == 3)
    count++;
  i = 
# 35 "./abi/pr60336-11.C" 3 4
     __builtin_va_arg(
# 35 "./abi/pr60336-11.C"
     va_arglist
# 35 "./abi/pr60336-11.C" 3 4
     ,
# 35 "./abi/pr60336-11.C"
     int
# 35 "./abi/pr60336-11.C" 3 4
     )
# 35 "./abi/pr60336-11.C"
                             ;
  if (i == 4)
    count++;
  i = 
# 38 "./abi/pr60336-11.C" 3 4
     __builtin_va_arg(
# 38 "./abi/pr60336-11.C"
     va_arglist
# 38 "./abi/pr60336-11.C" 3 4
     ,
# 38 "./abi/pr60336-11.C"
     int
# 38 "./abi/pr60336-11.C" 3 4
     )
# 38 "./abi/pr60336-11.C"
                             ;
  if (i == 5)
    count++;
  i = 
# 41 "./abi/pr60336-11.C" 3 4
     __builtin_va_arg(
# 41 "./abi/pr60336-11.C"
     va_arglist
# 41 "./abi/pr60336-11.C" 3 4
     ,
# 41 "./abi/pr60336-11.C"
     int
# 41 "./abi/pr60336-11.C" 3 4
     )
# 41 "./abi/pr60336-11.C"
                             ;
  if (i == 6)
    count++;
  
# 44 "./abi/pr60336-11.C" 3 4
 __builtin_va_end(
# 44 "./abi/pr60336-11.C"
 va_arglist
# 44 "./abi/pr60336-11.C" 3 4
 )
# 44 "./abi/pr60336-11.C"
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
