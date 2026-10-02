//type:fp
//options_all:--c++11
//remark:[6.7] Default member initializers and destructor instantiations
// 6/7/24   [EDGcpfe/27352]
//
// Default member initializers and destructor instantiations
//
// Previously, this elicited an error from the front end because the destructor
// needed for the destruction of S::m was instantiated when parsing the default
// member initializer.  Now, that instantiation is delayed until the destructor is
// actually needed.  (See also the entry for EDGcpfe/24790,EDGcpfe/25149, which
// covers the similar case for direct-list-initialization.)
template<typename T> struct X {
  ~X() { sizeof(T); }
};
struct Undefined;
struct S {
  X<Undefined> m = {};  // Previously an error.  Now okay.
};
