//type:fp
//options_all:--gn 100300 --c++17
//remark:[6.7] Internal error on __constinit in pre-C++20 GNU modes
// 12/22/23 [EDGcpfe/26908]
//
// Internal error on __constinit in pre-C++20 GNU modes
//
// An incorrectly-written assertion check in the front end caused it to fail that
// assertion in pre-C++20 GNU mode when a default-initialized __constinit variable
// has a nontrivial destructor.
//
// That is now fixed.
struct S { ~S(); };
__constinit S s;  // Previously triggered an internal error in some modes.
