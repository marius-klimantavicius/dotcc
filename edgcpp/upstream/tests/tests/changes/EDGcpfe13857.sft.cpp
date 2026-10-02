//type:fp
//options_all:--g++ --c++11
//remark:[4.7] Abort on out-of-class-template member function with noexcept-specifier
// 4/5/13   [EDGcpfe/13857]
//
// Abort on out-of-class-template member function with noexcept-specifier
//
// In C++11 modes that defer prototype instantiations of function templates
// (e.g., GNU C++-mode in many configurations), the front end aborted with an
// internal error in is_template_dependent_noexcept_specification (decls.c)
// when processing an out-of-class definition of a member function of a class
// template.
//
// This regression introduced by version 4.6 of the front end is now fixed.
template<class T>
class S { 
  S() noexcept(true); 
}; 
template<class T> S<T>::S() noexcept(true) {} 
  // Triggered an abort in GNU C++11 mode in version 4.6.
