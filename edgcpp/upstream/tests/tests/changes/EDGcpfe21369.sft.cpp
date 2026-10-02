//type:fp
//options_all:--gnu_version=80000 -w
//remark:[5.1] Explicit conversions in braced-initialization of references
// 6/12/19  [EDGcpfe/21369]
//
// Explicit conversions in braced-initialization of references
//
// In cases where a reference type is being initialized by braced-initialization
// and the initialization requires the invocation of an explicit user-defined
// conversion function, the front-end was failing to consider these explicit
// user-defined conversion functions.  This resulted in an error.
//
// This is now fixed.
struct A {
  explicit operator int&&();
};
void f() {
  int&& irr{ A{} }; // Previously issued a "no suitable conversion" error
}
