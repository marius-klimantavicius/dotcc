//type:fp
//options_all:--g++
//remark:[4.8] GNU compatibility: Floating-point literals in integral constant expressions
// 9/6/13   [EDGcpfe/14475]
//
// GNU compatibility: Floating-point literals in integral constant expressions
//
// In GNU modes, the front end now accepts floating-point literals in more
// contexts expecting integral constant expressions.
enum E { e = 2.1 ? 1 : 2 };  // Previously an error in many GNU modes;
                             // now accepted in all such modes.
