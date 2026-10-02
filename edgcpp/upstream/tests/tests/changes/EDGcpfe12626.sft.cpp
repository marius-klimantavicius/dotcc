//type:fn
//options_all:--c++11
//remark:[4.5] Abort on "explicit" specifier after error
// 2/22/12  [EDGcpfe/12626]
//
// Abort on "explicit" specifier after error
//
// In modes that permit explicit conversion functions (see Changes entry of
struct N {
  template<class T> struct X;
  inline explicit X();  // Not a valid declaration.  Previously aborted in
};                      // some modes.
