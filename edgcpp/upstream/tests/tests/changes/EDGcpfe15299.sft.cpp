//type:fn
//options_all:--c++11
//remark:[4.10] Function modifiers accepted in invalid syntactic locations
// 7/16/14  [EDGcpfe/15299]
//
// Function modifiers accepted in invalid syntactic locations
//
// The front end previously accepted the context-sensitive function modifiers
// "final" and "override" after function declarators that aren't for member
// function declarations.
//
// This is now fixed (i.e., an error is issued on such cases).
struct A {};
typedef void (A::*pmf)() final;  // Previously accepted in C++11 mode.
                                 // Now an error.
