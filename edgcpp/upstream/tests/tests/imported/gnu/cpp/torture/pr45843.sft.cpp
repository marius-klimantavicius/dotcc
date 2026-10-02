//type: rp
//options: 
# 0 "./torture/pr45843.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr45843.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./torture/pr45843.C" 2


# 6 "./torture/pr45843.C"
extern "C" void abort ();
struct S { struct T { } a[14]; char b; };
struct S arg, s;

void
foo (int z, ...)
{
  char c;
  va_list ap;
  
# 15 "./torture/pr45843.C" 3 4
 __builtin_va_start(
# 15 "./torture/pr45843.C"
 ap
# 15 "./torture/pr45843.C" 3 4
 ,
# 15 "./torture/pr45843.C"
 z
# 15 "./torture/pr45843.C" 3 4
 )
# 15 "./torture/pr45843.C"
                 ;
  c = 'a';
  arg = 
# 17 "./torture/pr45843.C" 3 4
       __builtin_va_arg(
# 17 "./torture/pr45843.C"
       ap
# 17 "./torture/pr45843.C" 3 4
       ,
# 17 "./torture/pr45843.C"
       struct S
# 17 "./torture/pr45843.C" 3 4
       )
# 17 "./torture/pr45843.C"
                            ;
  if (c != 'a')
    abort ();
  
# 20 "./torture/pr45843.C" 3 4
 __builtin_va_end(
# 20 "./torture/pr45843.C"
 ap
# 20 "./torture/pr45843.C" 3 4
 )
# 20 "./torture/pr45843.C"
            ;
}

int
main ()
{
  foo (1, s);
  return 0;
}
