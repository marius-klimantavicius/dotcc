//type:fp
//options_all:--gn 110999 --c
//remark:[6.4] C23 and GNU/Clang C compatibility: Unnamed parameters
// 9/27/22  [EDGcpfe/25631]
//
// C23 and GNU/Clang C compatibility: Unnamed parameters
//
// In C, unnamed parameters are not permitted in function definitions, but that
// restriction is expected to be lifted in the forthcoming C23 standard.  GCC and
// Clang have already lifted the restriction with their respective version 11.x.
// The front end therefore now reduces the corresponding diagnostic (ordinarily a
// discretionary error) to a warning in GNU C mode with gnu_version >= 110000 and
// Clang C mode with clang_version >= 110000.  Furthermore, in C23 modes the
// diagnostic is now omitted altogether.
  void f(int) {}  // Now accepted by default in some GCC and Clang C modes,
                  // as well as in all C23 modes.
