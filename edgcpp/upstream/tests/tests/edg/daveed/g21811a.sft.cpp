//remark:Error recovery
//options:;fn

  struct S {};
  S::S() {}  // Previously triggered an abort in some configurations.

