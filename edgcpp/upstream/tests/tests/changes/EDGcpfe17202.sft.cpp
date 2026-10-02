//type:fp
//options_all:--c++14
//remark:[4.12] C++14-mode abort on variable with an initializer that references itself
// 8/4/16   [EDGcpfe/17202]
//
// C++14-mode abort on variable with an initializer that references itself
//
// In C++14 mode, the front end sometimes aborted with an internal error in
// extract_value_from_constant (interpret.c) when processing the initializer for
// a constexpr variable that references the variable itself.
//
// This is now fixed.
//
// 8/4/16   [EDGcpfe/17202]
//
// Spurious failure to fold copying of literal object in C++14 interpreter
//
// In C++14 mode, the front end sometimes failed to fold the copying of a literal
// object produced by another constexpr call.
//
// Here the call to my was considered non-constant because the object produced by
// the call to mx was incorrectly treated as uninitialized.  This is now fixed.
constexpr int g(int const&) { return 0; }
constexpr int i = g(i);  // Previously triggered an internal error.
                         // Now okay.
