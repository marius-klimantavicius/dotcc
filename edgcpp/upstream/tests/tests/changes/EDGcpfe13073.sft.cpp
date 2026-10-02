//type:fp
//options_all:--g++
//remark:[4.5] GNU compatibility: Attribute "nonnull" and nonstatic member functions
// 8/16/12  [EDGcpfe/13073]
//
// GNU compatibility: Attribute "nonnull" and nonstatic member functions
//
// The front end now treats the "this" parameter of nonstatic member functions
// as parameter number "1" for the GNU attribute "nonnull".
struct S {
  void f(int*) __attribute((nonnull(2)));  // Previously, an error.  Now
};                                         // accepted.
