//type:fp
//options_all:--c23
//remark:[6.5] Digit separators in #elif expressions
// 3/21/23  [EDGcpfe/26170]
//
// Digit separators in #elif expressions
//
// In modes in which digit separators are accepted, the front end previously
// did not recognize them in numeric literals appearing in the expression of a
// #elif directive.  This is now fixed.
#if 0
#elif 0b0'111 == 7   // Previously a spurious error
int i;
#endif
