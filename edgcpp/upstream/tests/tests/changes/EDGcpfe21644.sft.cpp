//type:fn
//options_all:--c++14
//remark:[6.0] Constexpr explicit specialization of variable templates and initializers
// 8/15/19  [EDGcpfe/21644]
//
// Constexpr explicit specialization of variable templates and initializers
//
// The front end previously failed to diagnose missing initializers on constexpr
// explicit specializations of variable templates.
//
// That is now fixed.
template<typename T> inline T v = T();
template<> constexpr int v<int>;  // Previously accepted.  Now an error
                                  // because the constexpr variable is not
                                  // initialized.
