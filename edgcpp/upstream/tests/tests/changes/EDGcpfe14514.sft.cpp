//type:fp
//options_all:--microsoft
//remark:[4.8] Microsoft compatibility: dllimport variables as nontype template arguments
// 9/30/13  [EDGcpfe/14514]
//
// Microsoft compatibility: dllimport variables as nontype template arguments
//
// In Microsoft C++ mode variables declared as "dllimport" are usually treated as
// having a nonconstant address (see entry of 7/5/06).  As a consequence, their
// address cannot be used as a nontype template argument.  Now, however, the front
// end has lifted that restriction for variables of array type.
template<char* c> struct S {};
extern char __declspec(dllimport) x[];
typedef S<x> SX;  // Previously an error; now accepted.
