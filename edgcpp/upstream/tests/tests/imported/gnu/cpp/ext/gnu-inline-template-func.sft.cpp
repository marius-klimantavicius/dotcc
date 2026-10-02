//type: fp
//options: 
# 0 "./ext/gnu-inline-template-func.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/gnu-inline-template-func.C"
# 11 "./ext/gnu-inline-template-func.C"
# 1 "./ext/gnu-inline-global.C" 1
# 22 "./ext/gnu-inline-global.C"
# 1 "./ext/gnu-inline-common.h" 1
# 23 "./ext/gnu-inline-global.C" 2



template <typename T> __attribute__((gnu_inline)) inline int func1 (void) { return 0; }
template <typename T> int func1 (void) { return 2; }



template <typename T> extern int func2 (void);
template <typename T> __attribute__((gnu_inline)) inline int func2 (void) { return 0; }
template <typename T> int func2 (void) { return 2; }



template <typename T> extern int func3 (void);
template <typename T> __attribute__((gnu_inline)) inline int func3 (void) { return 0; }



template <typename T> extern int func4 (void);
template <typename T> __attribute__((gnu_inline)) inline int func4 (void) { return 0; }
template <typename T> int func4 (void) { return 1; }



template <typename T> static int func5 (void);
template <typename T> __attribute__((gnu_inline)) inline int func5 (void) { return 0; }
template <typename T> int func5 (void) { return 1; }
# 12 "./ext/gnu-inline-template-func.C" 2

template int func1<int>(void);
template int func2<int>(void);
template int func3<int>(void);
template int func4<int>(void);
template int func5<int>(void);
