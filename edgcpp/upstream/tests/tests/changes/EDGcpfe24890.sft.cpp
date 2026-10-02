//type:fp
//options_all:--c++17
//remark:[6.5] Incorrect lookup in lambda expression in static data member template
// 5/4/23   [EDGcpfe/24890]
//
// Incorrect lookup in lambda expression in static data member template
// initializer
//
// Previously, when a generic lambda expression was used in the initializer of a
// static data member template, lookup skipped the template parameter scope of
// that static data member template.
struct C {
  template<int I>
  static inline auto l = [] (auto j) {
    return I + j;  // Previously a spurious 'identifier "I" is undefined'
  };               // error.  Now okay.
};
auto v = C::l<1>(1);
