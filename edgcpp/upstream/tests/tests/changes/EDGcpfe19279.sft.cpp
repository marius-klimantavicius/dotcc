//type:fn
//options_all:--c++14
//remark:[5.0] Default arguments of generic lambdas not prototype-instantiated
// 2/6/18   [EDGcpfe/19279]
//
// Default arguments of generic lambdas not prototype-instantiated
//
// The front end previously failed to parse the default arguments of a generic
// lambda unless a real instantiation (as opposed to a prototype instantiation)
// was performed.
//
// Previously, the ill-formed default argument "int" was not parsed and therefore
// no diagnostic was emitted.  That is now fixed.
int r = [](auto x = int){ return x; }(1);  // Previously accepted.
                                           // Now an error.
