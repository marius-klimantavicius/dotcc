//type:fp
//options_all:--c++20
//remark:[6.7] Core issue 2619: Designators and direct-initialization
// 10/24/24 [EDGcpfe/25755,EDGcpfe/25853,EDGcpfe/27195,EDGcpfe/27261,
//           EDGcpfe/27545,EDGcpfe/27675]
//
// Core issue 2619: Designators and direct-initialization
//
// Previously, the front end treated all aggregate element initializations as
// "copy initialization", which discards "explicit" constructors, and thus the
// example above produced an error.  Core issue 2619 clarified that a designated
// initializer using direct-list-initialization syntax (i.e., without an
// intervening "=" token) is a form of "direct initialization", which does
// end now implements that issue resolution.
struct V { explicit V() {}; };
struct S { V v; } s = { .v{} };  // Previously an error.  Now okay.
