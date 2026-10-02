//type:fp
//options_all:--c++17
//remark:[6.8] CTAD from a brace-enclosed, cv-qualified instance of a class template
// 6/5/25   [EDGcpfe/24291]
//
// CTAD from a brace-enclosed, cv-qualified instance of a class template
//
// When deducing the type of a class template from a brace-enclosed element whose
// type is an instance of that class template, initializer-list constructors are
// omitted during the first phase of overload resolution.  Previously, the front
// end failed to do this for a cv-qualified element type or when the element type
// was denoted by a typedef name.
#include <initializer_list>
template<typename T>
struct C {
  C(std::initializer_list<T>);
};
C<int> f(const C<int> &c) {
  return C{c};  // Previously a spurious error.  Now okay.
}
