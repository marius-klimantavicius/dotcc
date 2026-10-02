//type:fn
//options_all:--g++
//remark:[4.4] GNU compatibility: Redeclarations of "gnu_inline" functions
// 10/5/11  [EDGcpfe/11323]
//
// GNU compatibility: Redeclarations of "gnu_inline" functions
//
// In GNU mode with gnu_version >= 40300, the front end now issues an error if
// a function first declared with the gnu_inline attribute does not specify that
// attribute on subsequent redeclarations that are explicitly "inline".
//
// 9/30/11  [EDGcpfe/11323]
//
// GNU compatibility: Attribute names prefixed with double underscore
//
// Previously, the front end treated GNU-style attributes whose name was prefixed
// or prefixed-and-suffixed with double underscores ("__") like the same name
// without those double underscores.  Now, GNU attribute names that are prefixed
// but not suffixed by a double underscore are no longer implicitly equivalent to
// the non-prefixed name.
__attribute((gnu_inline)) __inline__ void g();
__inline__ void g() {}  // Now an error in some GNU modes because the
                        // gnu_inline attribute is missing.
