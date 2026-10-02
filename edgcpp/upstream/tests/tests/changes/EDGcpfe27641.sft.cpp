//type:fp
//options_all:--c++11
//remark:[6.7] Non-trailing function parameter packs
// 10/16/24 [EDGcpfe/27641]
//
// Non-trailing function parameter packs
//
// The resolution of Core issue 1388 (treated as a defect report against previous
// C++ standards) requires that the types of non-trailing function parameter packs
// never get deduced.
template<typename>
struct C {
  C(int);
};
template<typename ... Ts>
int f(Ts ..., C<Ts ...>);
int i = f<int>(1, 2);  // Previously a spurious error.  Now okay.
