//type: fn
//options: 
// PR c++/14401

struct S {
  S() {} // { dg-error "" }
  const int i;
};
