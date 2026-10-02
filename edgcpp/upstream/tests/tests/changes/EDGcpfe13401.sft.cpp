//type:fp
//options_all:--g++
//remark:[4.6] GNU mode attribute and template parameters
// 11/16/12 [EDGcpfe/13401]
//
// GNU mode attribute and template parameters
//
// Previously, the front end issued an error when applying the GNU attribute
// "mode" to a template parameter type.  Now, this is accepted (an error may
// be issued at instantiation time if the type after substitution is invalid).
template<typename T> struct S {
  typedef T IntS __attribute((mode(SI)));  // Now accepted in GNU C++ mode.
};
