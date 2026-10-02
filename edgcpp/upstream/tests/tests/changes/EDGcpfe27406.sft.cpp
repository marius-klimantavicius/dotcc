//type:fp
//options_all:--c++20 --g++
//remark:[6.7] Abort on class template argument deduction for alias templates
// 7/3/24   [EDGcpfe/27406]
//
// Abort on class template argument deduction for alias templates
//
// In modes where the deduced type of a deduction guide can include CV qualifiers
// or name an alias template specialization, forming the guides for an alias
// template previously resulted in an incorrect variant access.
// --c++20 --g++:
template<typename> struct C {
  C(int);
};
template<typename T> C(T) -> const C<void>;
template<typename T> using A = C<T>;
A a{1};  // Previously triggered an abort.  Now okay.
