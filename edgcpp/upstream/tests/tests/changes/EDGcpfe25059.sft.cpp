//type:fp
//options_all:--microsoft
//remark:[6.4] Microsoft compatibility: string literal concatenation in comment _Pragma
// 2/24/22  [EDGcpfe/25059]
//
// Microsoft compatibility: string literal concatenation in comment _Pragma
//
// MSVC performs string literal concatenation, as well as substitution of
// function name variables, inside the string operand of a comment _Pragma
// directive.  The front end previously did not do so but has now been changed
// to emulate the Microsoft compiler's processing.
// --microsoft:
//
// previously resulted in an error expecting a ")" at the __FUNCTION__ token
// but now is accepted and is equivalent to
void foo() {
  _Pragma("comment(linker, \"/EXPORT:\" __FUNCTION__ \"=\" __FUNCDNAME__)")
}
