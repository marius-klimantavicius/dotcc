//source_files: 
//type: lp
//options: 
# 0 "./analyzer/stdarg-lto-1-a.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/stdarg-lto-1-a.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./analyzer/stdarg-lto-1-a.c" 2
# 1 "./analyzer/stdarg-lto-1.h" 1

# 1 "./analyzer/stdarg-lto-1.h"
extern void called_by_test_type_mismatch_1 (int placeholder, ...);
# 8 "./analyzer/stdarg-lto-1-a.c" 2



void
called_by_test_type_mismatch_1 (int placeholder, ...)
{
  const char *str;

  va_list ap;
  
# 17 "./analyzer/stdarg-lto-1-a.c" 3 4
 __builtin_c23_va_start(
# 17 "./analyzer/stdarg-lto-1-a.c"
 ap, placeholder
# 17 "./analyzer/stdarg-lto-1-a.c" 3 4
 )
# 17 "./analyzer/stdarg-lto-1-a.c"
                           ;

  str = 
# 19 "./analyzer/stdarg-lto-1-a.c" 3 4
       __builtin_va_arg(
# 19 "./analyzer/stdarg-lto-1-a.c"
       ap
# 19 "./analyzer/stdarg-lto-1-a.c" 3 4
       ,
# 19 "./analyzer/stdarg-lto-1-a.c"
       const char *
# 19 "./analyzer/stdarg-lto-1-a.c" 3 4
       )
# 19 "./analyzer/stdarg-lto-1-a.c"
                                ;

  
# 21 "./analyzer/stdarg-lto-1-a.c" 3 4
 __builtin_va_end(
# 21 "./analyzer/stdarg-lto-1-a.c"
 ap
# 21 "./analyzer/stdarg-lto-1-a.c" 3 4
 )
# 21 "./analyzer/stdarg-lto-1-a.c"
            ;
}

int main() { return 0; }
