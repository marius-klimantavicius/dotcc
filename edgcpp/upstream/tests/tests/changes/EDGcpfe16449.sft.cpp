//type:fp
//options_all:--gnu=40800
//remark:[4.11] Assertion failure in end_potential_pack_expansion_context
// 9/2/15   [EDGcpfe/16449]
//
// Assertion failure in end_potential_pack_expansion_context
//
// A function template with multiple attribute arguments in an attribute list
// had caused an assertion failure in end_potential_pack_expansion_context.
// Now fixed.
template <class T> void f() __attribute__((abi_tag("foo", "bar")));
