//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft C++ compatibility: void parameters in template instantiations
// 12/11/08 [EDGcpfe/6674, EDGcpfe/8978, EDGcpfe/9401, EDGcpfe/9430]
//
// Microsoft C++ compatibility: void parameters in template instantiations
//
// In Microsoft C++ mode, an ordinary (i.e., nontemplate) member function of a
// class template is treated as a function taking no arguments if it has a single
// parameter whose type is void after instantiation.
template<typename T> struct S {
  int f(T);
};
int r = S<void>().f();  // Accepted in Microsoft C++ mode.
