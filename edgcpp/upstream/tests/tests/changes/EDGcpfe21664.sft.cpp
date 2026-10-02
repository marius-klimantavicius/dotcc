//type:fn
//options_all:--c++17
//remark:[6.0] __is_constructible and incomplete types
// 9/16/19  [EDGcpfe/21664]
//
// __is_constructible and incomplete types
//
// Previously, applying __is_constructible to an incomplete class or enumeration
// type -- either the constructed type or an operand type -- was accepted and
// resulted in a "false" value.  Now such cases elicit a diagnostic in non-GNU
// modes (or a deduction failure it they occur during template argument
// deduction).
//
// A similar change was made for __is_destructible.
struct S;
bool r = __is_constructible(int, S);  // Previously okay (with FALSE value).
                                      // Now an error in non-GNU modes.
