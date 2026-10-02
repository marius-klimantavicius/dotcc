//type:fn
//options_all:--c++11 --clang
//remark:[5.0] Replaceable operator new/delete declarations and exception specifications
// 7/18/18  [EDGcpfe/19210]
//
// Replaceable operator new/delete declarations and exception specifications
//
// Previously, the front end always accepted this for backward compatibility
// reasons.  However, since C++11, the code is strictly-speaking invalid because
// "operator delete" is predeclared as "noexcept(true)".  In strict C++ mode and
// in Clang C++ modes the front end now enforces the standard redeclaration rule
// when implicit_noexcept_enabled is TRUE (see also the entry of 9/3/12 for
// EDGcpfe/10617, etc.): In such modes an error is issued for the example above.
// This change can also affect "operator new" declarations.
void* operator new(decltype(sizeof(2))) noexcept(true);
  // Now an error because this "operator new" is implicitly predeclared
  // with "noexcept(false)".
