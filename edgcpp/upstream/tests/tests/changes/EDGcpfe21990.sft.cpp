//type:fp
//options_all:--c++17
//remark:[6.0] Abort on literals in list initializers during variadic template prescan
// 11/19/19 [EDGcpfe/21990]
//
// Abort on literals in list initializers during variadic template prescan
//
// The change for EDGcpfe/21187 (in version 5.1) caused an abort in
// prep_generic_operand_full during variadic template prototype instantiation and
// prescanning template arguments involving list initializers containing literals.
//
// This is now fixed.
template <typename T> void foo(T x){}
template <typename T>
struct S2 {
   using type = T;
};
void bar() {
   foo<typename ::S2<decltype(long{1234})>::type>(long{1234});
   // The above line would previously cause an assertion failure
}
