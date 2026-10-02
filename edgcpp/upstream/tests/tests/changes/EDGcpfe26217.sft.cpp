//type:fp
//options_all:--c++17
//remark:[6.5] Default arguments for non-type template parameters of reference to auto type
// 4/17/23  [EDGcpfe/26217]
//
// Default arguments for non-type template parameters of reference to auto type
//
// When using a default template argument for a non-type template parameter of
// reference to auto type, the front end previously checked whether the type of
// the default template argument was valid as a non-type template parameter type
// instead of the deduced template parameter type.
struct B { B(); };
B b;
template<auto &v = b>
int f() { return 0; }
int i = f();  // Previously a spurious error.  Now okay.
