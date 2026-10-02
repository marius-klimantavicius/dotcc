//type:fp
//options_all:--microsoft
//remark:[4.0] Microsoft compatibility: Union with struct containing a flexible array member
// 10/23/08 [EDGcpfe/9309]
//
// Microsoft compatibility: Union with struct containing a flexible array member
//
// Various modes, including Microsoft modes, accept a class or struct type whose
// last field has a class type with a flexible array member.  In Microsoft mode,
// a class type with a flexible member can also appear in union types, and in
// those cases the flexible member can be any member of the union (not just the
// last one).  In Microsoft bugs mode, the front end now allows a field of such
// a union type to be followed by another field, if the field with the flexible
// member in the union is not the last field.  (This matches the behavior of
// Microsoft compilers.)
struct F {
  float f[];
};
union X {
  F fs;  // Flexible member in a union, but not the last field.
  int i;
};
struct Y {
  union X x;  // Accepted in Microsoft bugs mode, even though x is not the
  int y;      // last field.
};
