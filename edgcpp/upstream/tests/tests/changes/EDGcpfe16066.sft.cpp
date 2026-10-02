//type:fp
//options_all:--c --clang
//remark:[4.10.1] Clang compatibility: aggregate initialization of _Complex in C mode
// 3/6/15  [EDGcpfe/16066]
//
// Clang compatibility: aggregate initialization of _Complex in C mode
//
// The clang compiler allows aggregate initialization of a _Complex variable
// in C mode (though GNU does not).  Previously this had caused an assertion
// failure (in lower_c99_constant).
_Complex float x = { 1.0f, 1.0f };  // Now accepted in Clang C mode.
