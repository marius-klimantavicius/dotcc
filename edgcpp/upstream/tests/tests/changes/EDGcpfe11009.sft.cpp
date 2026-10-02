//type:fp
//remark:[4.3] Spurious errors on uses of parameters declared as arrays of const elements
// 9/16/10  [EDGcpfe/11009]
//
// Spurious errors on uses of parameters declared as arrays of const elements
//
// A bug introduced in version 4.2 caused the front end to incorrectly treat a
// parameter declared with a type "array of const T" as being a "const" (i.e.,
// immutable) parameter.  This resulted in spurious errors.
//
// This is now fixed.
void f(char const p[]) {
  while (*p++);  // Version 4.2 incorrectly issued an error because it
}                // considered p to be "const".
