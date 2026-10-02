//type:fp
//options_all:--c++11
//remark:[4.11] Array to pointer decay in dependent constant expressions
// 11/6/15  [EDGcpfe/15972]
//
// Array to pointer decay in dependent constant expressions
//
// The front end previously erroneously treated an array to pointer decay as
// making a dependent expression non-constant.  Such expressions are now
// accepted as constant during prototype instantiation.
// --c++11:
template<typename T1, typename T2> struct A {
  static constexpr bool v = true;
};
template<typename T> struct B {
  typedef T t;
};
template<typename T> struct C {
  static constexpr const char* const x =
    A<T, typename B<T>::t>::v ? "t" : "f";  // Previously not constant
                                            // because of array to ptr decay
};
