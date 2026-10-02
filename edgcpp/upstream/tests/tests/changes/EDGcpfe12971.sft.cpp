//type:fp
//options_all:--microsoft
//remark:[4.11] Microsoft compatibility: Flexible array members in unions (IL CHANGE)
// 8/11/15  [EDGcpfe/12971]
//
// Microsoft compatibility: Flexible array members in unions (IL CHANGE)
//
// Ordinarily, a flexible array member in a class type prevents that class type
// from being used for a field that is not the last field of a class or struct
// type, or as the underlying element type of an array.  However, those
// restrictions are now lifted in Microsoft mode if the class type is a union
// type and the flexible member is not the last member of the union.
//
// As a part of this change, the flag contains_flexible_array_member is no longer
// set to TRUE for such union types.  This is a small IL CHANGE.
union U {
  char c[];
  int  i;
};
U x[2];  // Now accepted in Microsoft mode.
