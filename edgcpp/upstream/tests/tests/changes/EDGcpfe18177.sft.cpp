//type:fp
//options_all:--microsoft_version 1903
//remark:[5.1] Microsoft compatibility: dllimport static data member initializers
// 10/10/18 [EDGcpfe/18177]
//
// Microsoft compatibility: dllimport static data member initializers
//
// The front end no longer issues an error for a dllimport static data member with
// an in-class initializer.
struct __declspec(dllimport) S {
  static constexpr int N = 32;  // Previously an error.  Now okay.
};
