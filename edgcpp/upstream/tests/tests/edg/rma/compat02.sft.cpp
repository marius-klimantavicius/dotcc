//options_all:-r -x -tused
//options: --strict;cp

template <class T> struct A {
  static int a[5];
};
template <class T> int A<T>::a[] = {1, 2};
A<int> x;


