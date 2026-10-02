//type:fp
//options_all:--c++17 --gn 80000
//remark:[5.1] Copy elision in direct-initialization context
// 1/31/19  [EDGcpfe/20835]
//
// Copy elision in direct-initialization context
//
// C++17 included changes that amounted to "mandatory copy elision": Copying a
// prvalue generally just causes the prvalue to be constructed directly in its
// final destination.  Previously, however, that was not always correctly applied
// in direct-initialization contexts.
//
// That is now fixed.
struct S { S(); S(S const&) = delete; };
S obj{S{}};  // Previously an error because the copy constructor
             // invocation was not elided.  Now okay in C++17 mode.
