//type: fp
//options: 
# 0 "./analyzer/stdarg-lto-1-b.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/stdarg-lto-1-b.c"
# 1 "./analyzer/stdarg-lto-1.h" 1
extern void called_by_test_type_mismatch_1 (int placeholder, ...);
# 2 "./analyzer/stdarg-lto-1-b.c" 2

void test_type_mismatch_1 (void)
{
  called_by_test_type_mismatch_1 (42, 1066);
}
