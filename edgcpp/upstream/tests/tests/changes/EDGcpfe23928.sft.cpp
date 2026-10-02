//type:fp
//options_all:--c++20 --il_display
//remark:[6.3] Uninitialized end-position in C++20 parenthesized aggregate initialization
// 2/19/21  [EDGcpfe/23928]
//
// Uninitialized end-position in C++20 parenthesized aggregate initialization
//
// Here, the sub-expression "S()" is treated as a parenthesized aggregate
// initialization, and ultimately represented using an enk_temp_init node.
// However, the end-position of the sub-expression (recorded when the macro
// EXTRA_SOURCE_POSITIONS_IN_IL is TRUE), was previously set from an
// uninitialized value, causing the enk_temp_init node's expr_range field to
// be incorrect.  That is now fixed.
struct S {};
void f(S) {}
void g() {
  f(S());  // Incorrect "end position" for node representing S().
}
