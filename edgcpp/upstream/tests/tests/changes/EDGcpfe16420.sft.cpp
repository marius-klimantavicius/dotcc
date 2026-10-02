//type:fn
//options_all:--c++11
//remark:[4.12] Multiple default union member initializers via anonymous unions
// 9/16/16  [EDGcpfe/16420]
//
// Multiple default union member initializers via anonymous unions
//
// The front end previously failed to diagnose unions with multiple default
// member initializers if one of the members was declared within an anonymous
// union.  Having multiple initializers in the IL could then cause expression
// folding to abort later on.
//
// This is now fixed.
union U {
  int i = 1;
  constexpr U() {}
  union { float f = 2.0; };  // Previously not diagnosed.  Now an error.
};
float r = U().f;  // Previously triggered an abort in folding.c.
