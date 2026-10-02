//type:fp
//options_all:--c++17
//remark:[5.0] Fold expressions and packs referenced in template arguments
// 10/24/17 [EDGcpfe/18887]
//
// Fold expressions and packs referenced in template arguments
//
// In some cases, the front end issued spurious errors (e.g., about a pack being
// referenced but not expanded) if a C++17 fold expression referred to a template
// parameter pack from within a template argument list.
//
// This is now fixed.
template<int> void ft();
template<int... Ns> void g() {
  (ft<1+Ns>(), ...);  // Previously triggered a series of spurious errors.
}
