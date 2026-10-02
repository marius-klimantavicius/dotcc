//type:fp
//options_all:--microsoft
//remark:[4.7] Microsoft C++: Specializing a file-scope class template in a namespace
// 4/19/13  [EDGcpfe/13704]
//
// Microsoft C++: Specializing a file-scope class template in a namespace
//
// In Microsoft C++ mode, a specialization for a file-scope class template is now
// accepted in a namespace.
//
// 4/18/13  [EDGcpfe/13704]
//
// GNU and Microsoft C++: access ignored on unevaluated copy constructor after
//
// g++ and MSVC++ do not appear to check the access on a copy constructor
// needed to copy an argument after it has been converted via a conversion
// function that returns a reference, in an unevaluated expression (in
// g++, an unevaluated expression in a template).  That is now emulated in
// those modes.
template<class T> struct S {};
namespace N {
  template<> struct S<int> {};  // Now accepted in Microsoft C++ modes.
}
