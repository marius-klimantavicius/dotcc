//type:fp
//options_all:--c11
//remark:[4.9] C11: _Generic
// 1/24/14  [EDGcpfe/14743]
//
// C11: _Generic
//
// The front end now supports the C11 _Generic construct.
//
// A new configuration macro REPRESENT_C11_GENERIC_CONSTRUCT_IN_IL determines
// whether the construct as a whole is represented in the IL (if the macro is
// TRUE) or if only the selected expression is represented (if the macro is
// FALSE).  The macro is TRUE by default if IL lowering is disabled and FALSE
// otherwise (if it is set to TRUE when lowering is enabled, lowering will
// eliminate the construct).
int f();
int x = _Generic(f(), signed: -1, unsigned: 1);
  // Now accepted in C11 mode and equivalent to "int x = -1;".
