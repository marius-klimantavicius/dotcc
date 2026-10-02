//type:fn
//remark:[6.8] Complete-type requirement for __underlying_type
// 9/5/25   [EDGcpfe/28420]
//
// Complete-type requirement for __underlying_type
//
// The type trait helper __underlying_type previously failed to enforce the
// requirement that the given enumeration type is complete.  That is now fixed.
enum E {
  e = __underlying_type(E)()  // Previously accepted.  Now an error.
};
