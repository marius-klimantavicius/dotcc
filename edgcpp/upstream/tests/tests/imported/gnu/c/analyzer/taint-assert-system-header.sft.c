//type: fp
//options: 
# 0 "./analyzer/taint-assert-system-header.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/taint-assert-system-header.c"
# 12 "./analyzer/taint-assert-system-header.c"
# 1 "./analyzer/test-assert.h" 1
       
# 2 "./analyzer/test-assert.h" 3


# 3 "./analyzer/test-assert.h" 3
extern void __assert_fail (const char *expr, const char *file, int line)
  __attribute__ ((__noreturn__));
# 13 "./analyzer/taint-assert-system-header.c" 2


# 14 "./analyzer/taint-assert-system-header.c"
int __attribute__((tainted_args))
test_tainted_assert (int n)
{
  
# 17 "./analyzer/taint-assert-system-header.c" 3
 do { if (!(
# 17 "./analyzer/taint-assert-system-header.c"
 n > 0
# 17 "./analyzer/taint-assert-system-header.c" 3
 )) __assert_fail (
# 17 "./analyzer/taint-assert-system-header.c"
 "n > 0"
# 17 "./analyzer/taint-assert-system-header.c" 3
 , "./analyzer/taint-assert-system-header.c", 17); } while (0)
# 17 "./analyzer/taint-assert-system-header.c"
               ;
  return n * n;
}
