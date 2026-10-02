//type: fp
//options: 
# 0 "./ext/gnu-inline-namespace.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/gnu-inline-namespace.C"
# 9 "./ext/gnu-inline-namespace.C"
namespace gnu_test {
# 1 "./ext/gnu-inline-global.C" 1
# 22 "./ext/gnu-inline-global.C"
# 1 "./ext/gnu-inline-common.h" 1
# 23 "./ext/gnu-inline-global.C" 2



 __attribute__((gnu_inline)) inline int func1 (void) { return 0; }
 int func1 (void) { return 2; }



 extern int func2 (void);
 __attribute__((gnu_inline)) inline int func2 (void) { return 0; }
 int func2 (void) { return 2; }



 extern int func3 (void);
 __attribute__((gnu_inline)) inline int func3 (void) { return 0; }



 extern int func4 (void);
 __attribute__((gnu_inline)) inline int func4 (void) { return 0; }
 int func4 (void) { return 1; }



 static int func5 (void);
 __attribute__((gnu_inline)) inline int func5 (void) { return 0; }
 int func5 (void) { return 1; }
# 11 "./ext/gnu-inline-namespace.C" 2
}
