//remark:Null pointer nontype template arguments
//options:--microsoft_version=1927;fp:--microsoft_version=1928;fn


  template<int*> struct S {};
  S<0> s;  // Previously accepted in all Microsoft C++ modes.
