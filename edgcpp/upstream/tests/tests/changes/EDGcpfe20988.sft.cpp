//type:fp
//options_all:--gn 70100 --c++17
//remark:[6.1] Class template argument deduction for variable templates
// 7/20/20  [EDGcpfe/20988,EDGcpfe/23131]
//
// Class template argument deduction for variable templates
//
// The front end previously often failed parsing variable template definitions
// that relied on class template argument deduction.
//
// That is now fixed.
template<typename> struct X {};
template<typename T> X<T> f() { return X<T>{}; }
template<typename T>
X vt = f<T>();  // Previously failed to parse.  Now okay.
