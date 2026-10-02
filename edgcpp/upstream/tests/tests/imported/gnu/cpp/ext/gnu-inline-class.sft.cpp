//type: fp
//options: 
# 0 "./ext/gnu-inline-class.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/gnu-inline-class.C"
# 11 "./ext/gnu-inline-class.C"
struct gnu_test {
  int func1(void);
  int func2(void);
  int func3(void);
  int func4(void);
  int func5(void);
};

# 1 "./ext/gnu-inline-global.C" 1
# 22 "./ext/gnu-inline-global.C"
# 1 "./ext/gnu-inline-common.h" 1
# 23 "./ext/gnu-inline-global.C" 2



 __attribute__((gnu_inline)) inline int gnu_test::func1 (void) { return 0; }
 int gnu_test::func1 (void) { return 2; }




 __attribute__((gnu_inline)) inline int gnu_test::func2 (void) { return 0; }
 int gnu_test::func2 (void) { return 2; }




 __attribute__((gnu_inline)) inline int gnu_test::func3 (void) { return 0; }




 __attribute__((gnu_inline)) inline int gnu_test::func4 (void) { return 0; }
 int gnu_test::func4 (void) { return 1; }




 __attribute__((gnu_inline)) inline int gnu_test::func5 (void) { return 0; }
 int gnu_test::func5 (void) { return 1; }
# 20 "./ext/gnu-inline-class.C" 2
