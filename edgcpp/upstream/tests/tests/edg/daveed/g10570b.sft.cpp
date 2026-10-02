//remark:Initialization of flexible array members
//options:--gcc;fn:-DPOS --gcc;fp

  typedef struct S { int n; int a[]; } S;
  void g() {
#ifndef POS
    S s1 = { 1, { 2 } };  // Previously accepted in GNU C mode; now an error.
#endif
    static S s2 = { 3, { 4 } };  // Still accepted in GNU C mode.
  }
