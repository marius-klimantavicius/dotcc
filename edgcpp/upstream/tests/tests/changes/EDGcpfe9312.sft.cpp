//type:fp
//options_all:--c --gcc
//remark:[4.0] GNU C compatibility: Unprototyped redeclarations
// 10/28/08 [EDGcpfe/9312]
//
// GNU C compatibility: Unprototyped redeclarations
//
// In GNU C mode with gnu_version < 40000, when a function is first declared with
// a prototyped declaration and then redeclared with an unprototyped declaration,
// type qualifier differences are ignored on the function's return type.
const int f(void);    // Prototyped declaration.
int f() { return 0; } // Unprototyped declaration: Missing "const" is
                      // accepted when gnu_version < 40000.
