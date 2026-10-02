//type: fp
//options: 
# 0 "./ext/gnu-inline-template-class.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/gnu-inline-template-class.C"
# 9 "./ext/gnu-inline-template-class.C"
template <typename T> struct gnu_test {
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



template <typename T> __attribute__((gnu_inline)) inline int gnu_test<T>::func1 (void) { return 0; }
template <typename T> int gnu_test<T>::func1 (void) { return 2; }




template <typename T> __attribute__((gnu_inline)) inline int gnu_test<T>::func2 (void) { return 0; }
template <typename T> int gnu_test<T>::func2 (void) { return 2; }




template <typename T> __attribute__((gnu_inline)) inline int gnu_test<T>::func3 (void) { return 0; }




template <typename T> __attribute__((gnu_inline)) inline int gnu_test<T>::func4 (void) { return 0; }
template <typename T> int gnu_test<T>::func4 (void) { return 1; }




template <typename T> __attribute__((gnu_inline)) inline int gnu_test<T>::func5 (void) { return 0; }
template <typename T> int gnu_test<T>::func5 (void) { return 1; }
# 21 "./ext/gnu-inline-template-class.C" 2

template struct gnu_test<int>;
