//type: fp
//options: 
# 0 "./abi/empty21.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/empty21.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./abi/empty21.C" 2


# 6 "./abi/empty21.C"
struct A { };

void f(int i, ...)
{
  va_list ap;
  
# 11 "./abi/empty21.C" 3 4
 __builtin_va_start(
# 11 "./abi/empty21.C"
 ap
# 11 "./abi/empty21.C" 3 4
 ,
# 11 "./abi/empty21.C"
 i
# 11 "./abi/empty21.C" 3 4
 )
# 11 "./abi/empty21.C"
                 ;
  if (i >= 1)
    
# 13 "./abi/empty21.C" 3 4
   __builtin_va_arg(
# 13 "./abi/empty21.C"
   ap
# 13 "./abi/empty21.C" 3 4
   ,
# 13 "./abi/empty21.C"
   A
# 13 "./abi/empty21.C" 3 4
   )
# 13 "./abi/empty21.C"
                 ;
  if (i >= 2)
    
# 15 "./abi/empty21.C" 3 4
   __builtin_va_arg(
# 15 "./abi/empty21.C"
   ap
# 15 "./abi/empty21.C" 3 4
   ,
# 15 "./abi/empty21.C"
   int
# 15 "./abi/empty21.C" 3 4
   )
# 15 "./abi/empty21.C"
                   ;
}

int main()
{
  f(0);
  f(1, A());
  f(2, A(), 42);
}
