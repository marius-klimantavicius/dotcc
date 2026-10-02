//type:fp
//remark:[5.1] Folding casts to virtual base classes (IL CHANGE)
// 1/11/19  [EDGcpfe/20716]
//
// Folding casts to virtual base classes (IL CHANGE)
//
// The front end previously did not fold casts to virtual base classes (with some
// minor exceptions).
//
// That is now fixed.  This fix involves an IL CHANGE to the representation of
// "subobject paths" (see also the entry for EDGcpfe/17710).
struct V {};
struct B: virtual V {};
struct D: B {} d;
constexpr V *p = (B*)&d;  // Previously an error.  Now okay.
