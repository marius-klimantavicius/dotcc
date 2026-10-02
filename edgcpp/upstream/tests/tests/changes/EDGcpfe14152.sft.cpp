//type:fp
//options_all:--c++14
//remark:[4.10] C++14: Generic Lambdas
// 12/22/14 [EDGcpfe/14152]
//
// C++14: Generic Lambdas
//
// In C++14 mode, the front end now supports generic lambdas.
//
// Generic lambdas were added to the working paper through the C++ standards
// committee's paper N3649.  The implicit capture rule adopted in that paper,
// however, is thought to be impractical: The front end therefore uses an
// approach that sometimes results in a different set of captures (though in the
// common cases, they are the same; other compilers implementing generic lambdas
// have similarly eschewed the standard rule).  It is therefore likely that the
// details of implicit captures in generic lambdas will be revised in the future.
auto dbl = [](auto p) { return 2*p; };
int x = dbl(2);
double y = dbl(2.0);
