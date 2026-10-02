//type:fn
//options_all:--c++17 -A
//remark:[5.1] Missing diagnostic (and potential abort) on structured binding templates
// 3/28/19  [EDGcpfe/20986]
//
// Missing diagnostic (and potential abort) on structured binding templates
//
// The front end previously failed to diagnose attempts at defining a structured
// binding template.  In some configurations, doing so could lead to an internal
// error in name mangling.
//
// That is now fixed.
float p[3];
template<typename T> auto [x, y, z] = p;  // Now an error.
