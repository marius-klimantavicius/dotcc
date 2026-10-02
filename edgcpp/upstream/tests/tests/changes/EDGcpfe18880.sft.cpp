//type:fp
//options_all:--c++14
//remark:[5.0] Abort in pop_object_lifetime_full with decltype(auto) variable declaration
// 10/24/17 [EDGcpfe/18880]
//
// Abort in pop_object_lifetime_full with decltype(auto) variable declaration
//
// Previously, certain decltype(auto) variable initializers requiring nontrivial
// object lifetime management could abort with an internal error in
// pop_object_lifetime_full.
//
// That is now fixed.
decltype(auto) x = new int(42);  // Previously triggered an internal error.
