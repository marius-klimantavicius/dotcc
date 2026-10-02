//type:fn
//options_all:--c++14
//remark:[4.12] Missing diagnostic on invalid use of "operator auto"
// 5/12/16  [EDGcpfe/17174]
//
// Missing diagnostic on invalid use of "operator auto"
//
// When deduced return types are enabled (e.g., in C++14 mode), the front end
// erroneously accepted explicit invocations of "operator auto" on an object with
// a conversion function template.
//
// This is now fixed (i.e., an error is now issued in such cases).
struct S { template<typename T> operator T(); };
void g() {
  S().operator auto();  // Previously accepted; now an error.
}
