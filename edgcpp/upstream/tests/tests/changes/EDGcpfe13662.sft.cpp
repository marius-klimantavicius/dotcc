//type:fp
//options_all:--g++
//remark:[4.6] GNU C++ compatibility: Folding of dependent built-in function calls
// 2/18/13  [EDGcpfe/13662]
//
// GNU C++ compatibility: Folding of dependent built-in function calls
//
// Previously the front end treated calls to GNU __builtin_... functions that
// involve template-dependent arguments as not constant.  This could result in
// spurious errors.
//
// Now calls to foldable GNU __builtin_... functions for which all arguments
// are constants are treated as potentially constant during generic template
// processing if at least one of the arguments is template-dependent in any way.
// (Such calls may still be considered nonconstant when they are instantiated.)
template <int N> struct X {
 static const int S = __builtin_clzll(N);  // Previously an error; now
};                                         // accepted.
