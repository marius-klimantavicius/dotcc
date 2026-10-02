//remark:GNU constant folding
//options:--gcc;fp:--g++;fp:--c99;fn

  int x;
  struct S { int i: 1+(x == x); }; // Now accepted in GNU modes.
