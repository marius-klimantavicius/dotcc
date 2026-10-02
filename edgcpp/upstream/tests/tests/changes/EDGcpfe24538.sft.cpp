//type:fp
//options_all:--ms_c++latest
//remark:[6.7] Spurious error on designator into anonymous struct within a union
// 10/4/24  [EDGcpfe/24538,EDGcpfe/27611]
//
// Spurious error on designator into anonymous struct within a union
//
// Previously, this elicited a spurious error due to a flaw in the logic guarding
// against multiple designators within a union.  That is now fixed.
union U {
  struct { int x, y; };
} u = { .x = 1, .y = 2 };  // Previously a spurious error.  Now okay.
