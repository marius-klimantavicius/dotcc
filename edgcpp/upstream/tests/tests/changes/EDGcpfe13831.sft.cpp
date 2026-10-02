//type:fn
//remark:[4.7] Aggregate initializers that fail to initialize a reference
// 4/5/13   [EDGcpfe/13831]
//
// Aggregate initializers that fail to initialize a reference
//
// The front end previously accepted aggregate initializers that didn't properly
// initialize a reference member if that member was embedded in an implicitly
// initialized aggregate type.
//
// Now such cases trigger an error.
struct R { int &r; };
struct S { int i; R x; } s = { 1 };  // Previously accepted; now an error
                                     // because s.x.r is a uninitialized
                                     // reference.
