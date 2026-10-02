//type:fp
//options_all:--c++20
//remark:[6.3] Spurious error on certain dependent uses of the unary "*" operator
// 5/26/21  [EDGcpfe/24360]
//
// Spurious error on certain dependent uses of the unary "*" operator
//
// The front end previously spuriously diagnosed the use of the unary "*" in
// this example.  That is now fixed.
template<typename T> concept D = requires(T &&p) {
  *(decltype(p)&&)p;  // Previously a spurious error.  Now okay.
};
