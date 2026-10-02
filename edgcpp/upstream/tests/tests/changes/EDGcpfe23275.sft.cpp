//type:fp
//options_all:--c++14
//remark:[6.6] Spurious 'argument list for variable template is missing' error during
// 7/25/23  [EDGcpfe/23275,EDGcpfe/25037,EDGcpfe/26189]
//
// Spurious 'argument list for variable template is missing' error during
// substitution
//
// When substitution resulted in a variable template without a template argument
// list, the front end would previously always issue a diagnostic instead of
// letting substitution fail.
template<typename T> auto f() -> decltype(T::v);
template<typename T> int f();
struct A {
  template<typename> static int v;
};
int i = f<A>();  // Previously a spurious 'argument list for variable
                 // template "A::v" is missing' error.  Now okay.
