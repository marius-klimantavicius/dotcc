//type:fn
//options_all:--c++17
//remark:[5.1] Lambdas in template arguments
// 8/24/18  [EDGcpfe/20049]
//
// Lambdas in template arguments
//
// The front end now diagnoses the use of lambdas in various contexts as required
// by the C++17 standard, including function template signatures and template
// arguments.
//
// (This is the example from EDGcpfe/19178.)
template <int A[+[]{return [=]{ return 42;}(); }() ]> void f () {}
  // Now an error.
