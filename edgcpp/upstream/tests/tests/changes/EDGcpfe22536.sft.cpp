//type:fp
//options_all:--gn 59999
//remark:[6.1] GNU/Clang compatibility: Use of "main" as a variable
// 3/24/20  [EDGcpfe/22536]
//
// GNU/Clang compatibility: Use of "main" as a variable
//
// As a result of the changes for EDGcpfe/17598 (in version 5.1), the use of
// "main" as a variable was disallowed in all modes.  That appears to be too
// restrictive as all versions of gcc and clang in C mode accept it, as well
// as early versions of both compilers in C++ mode.  A change has been made to
// more closely emulate that.
extern int main;  // warning in GNU/clang C modes
                  // warning in early GNU/clang C++ modes
                  // error in strict C++ mode
                  // discretionary error otherwise
