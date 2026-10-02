//type:fp
//remark:[4.5] Unnamed bit fields of enumeration type
// 5/9/12   [EDGcpfe/12580, EDGcpfe/12891]
//
// Unnamed bit fields of enumeration type
//
// In modes that accept enumeration definitions with explicit underlying types
// (e.g., C++11 modes), the front end incorrectly parsed unnamed bit fields
// declared with an elaborated enumeration type (it treated the colon of the bit
// field declaration as the introduction of the underlying type).
//
// This is now fixed.
enum E { e };
struct S {
  enum E: 8;  // Previously triggered spurious errors.
};
