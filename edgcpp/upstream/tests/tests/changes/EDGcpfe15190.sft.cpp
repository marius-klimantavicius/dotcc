//type:fp
//options_all:--c++11
//remark:[4.10] __func__ in templates
// 5/30/14  [EDGcpfe/15190]
//
// __func__ in templates
//
// The type of __func__ in a template is potentially dependent on the arguments
// for which the template is instantiated (i.e., the length of the string
// represented by __func__ can differ from one instantiation to the next), but
// previously the front end did not treat the expression __func__ as a template-
// dependent expression.  That in turn could result in spurious errors due to
// incorrect lookup results for certain function and operator calls that have
// __func__ as an argument (in C++11 mode).
//
// This is now fixed.  (The dependent nature of __func__ appearing in templates
// was clarified by the standards committee's resolution for Core issue 1779.)
struct Log {
  template<typename T> Log& operator<<(T const&);
};
template<class T> struct B {
  void f() {
    Log() << __func__; // Previously did not find instantiations of
  }                    // Log::operator<< during most real instantiations.
};
template struct B<int>;  // Triggered a spurious error.
