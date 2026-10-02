//type:fp
//options_all:--microsoft
//remark:[4.10] Microsoft compatibility: Duplicate definition of variables
// 4/23/14  [EDGcpfe/15048]
//
// Microsoft compatibility: Duplicate definition of variables
//
// In Microsoft modes, the front end now accepts two definitions for a non-local
// variable if the first definition does not include an initializer and the
// second definition includes an initializer and the "extern" storage class
// specifier.
//
// In such cases, the first definition is retroactively treated as a declaration.
// Variables that require nontrivial destruction are not considered for this
// treatment.
int x;
extern int x = 3;  // Now accepted in Microsoft modes.
