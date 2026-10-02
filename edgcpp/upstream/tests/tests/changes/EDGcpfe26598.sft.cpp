//type:fp
//options_all:--c++20
//remark:[6.7] Matching of constrained out-of-class member template declarations
// 1/22/24  [EDGcpfe/26598,EDGcpfe/26866]
//
// Matching of constrained out-of-class member template declarations
//
// Previously, the front end failed to match an out-of-class member template
// definition with its declaration if the class was constrained using a requires
// clause in its template header.
template<typename> struct B;
template<typename T> requires true
struct B<T> {
  template<int> struct N;
};
template<typename T> requires true
template<int> struct B<T>::N {};  // Previously a spurious error.  Now okay.
