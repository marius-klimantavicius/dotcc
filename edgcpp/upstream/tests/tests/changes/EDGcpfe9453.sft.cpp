//type:fp
//remark:[4.1] Spurious ambiguity when declaring a conversion operator template instance
// 4/2/09   [EDGcpfe/9453]
//
// Spurious ambiguity when declaring a conversion operator template instance
//
// When explicitly specializing a conversion operator template belonging to a set
// of member function templates only distinguished by cv-qualification, the front
// end issued a spurious error.
//
// This spurious error could also occur with other contexts that declare a
// conversion template instance (e.g., friend declarations).  This is now fixed.
struct S {
  template<class T> operator T();
  template<class T> operator T() const;
};
template<> S::operator int() const;  // Previously resulted in a spurious
                                     // error.
