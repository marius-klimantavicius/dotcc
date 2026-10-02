//type:fn
//remark:[4.5] Abort on invalid pointer-to-member
// 4/20/12  [EDGcpfe/12794]
//
// Abort on invalid pointer-to-member
//
// The diagnosis of certain invalid pointer-to-member types resulted in an abort
// in form_type_first_part (il_to_str.c).
//
// This is now fixed.
struct A {};
void (A::*& (A::*x))();  // Previously triggered an abort.
                         // (This pointer-to-member is invalid because the
                         // member type is a reference type.)
