//type:fp
//options_all:--c++11
//remark:[4.7] Abort on nested class derived from dependent parent class (GNU mode)
// 5/1/13   [EDGcpfe/13918]
//
// Abort on nested class derived from dependent parent class (GNU mode)
//
// In GNU C++ mode, a nested class defined in a class template can derive from a
// dependent instance of the class template.  Version 4.6 of the front end,
// however, introduced a bug causing such cases to abort with an internal error
// (in is_literal_type) in GNU C++11 mode.
//
// This is now fixed.
template<typename T> struct S {
  struct N: S<T> {};  // Previously triggered an internal error in
};                    // GNU C++11 mode.
