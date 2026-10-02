//type:fn
//options_all:--c++17
//remark:[6.8] Assertion failure in make_static_assert_string_for_output
// 8/26/25  [EDGcpfe/28412]
//
// Assertion failure in make_static_assert_string_for_output
//
// In cases where a static_assert is not terminated by a semicolon but is followed
// by a preprocessing directive, an assertion failure (in
// make_static_assert_string_for_output) could occur and is now fixed.
static_assert(false, "a string")    // Missing semicolon
#if defined(SOME_DEF)
#endif
