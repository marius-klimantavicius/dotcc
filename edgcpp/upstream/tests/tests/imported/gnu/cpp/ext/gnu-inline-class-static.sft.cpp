//type: fp
//options: 
# 0 "./ext/gnu-inline-class-static.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/gnu-inline-class-static.C"
# 12 "./ext/gnu-inline-class-static.C"
struct gnu_test_static {
  static int func1(void);
  static int func2(void);
  static int func3(void);
  static int func4(void);
  static int func5(void);
};

# 1 "./ext/gnu-inline-global.C" 1
# 22 "./ext/gnu-inline-global.C"
# 1 "./ext/gnu-inline-common.h" 1
# 23 "./ext/gnu-inline-global.C" 2



 __attribute__((gnu_inline)) inline int gnu_test_static::func1 (void) { return 0; }
 int gnu_test_static::func1 (void) { return 2; }




 __attribute__((gnu_inline)) inline int gnu_test_static::func2 (void) { return 0; }
 int gnu_test_static::func2 (void) { return 2; }




 __attribute__((gnu_inline)) inline int gnu_test_static::func3 (void) { return 0; }




 __attribute__((gnu_inline)) inline int gnu_test_static::func4 (void) { return 0; }
 int gnu_test_static::func4 (void) { return 1; }




 __attribute__((gnu_inline)) inline int gnu_test_static::func5 (void) { return 0; }
 int gnu_test_static::func5 (void) { return 1; }
# 21 "./ext/gnu-inline-class-static.C" 2
