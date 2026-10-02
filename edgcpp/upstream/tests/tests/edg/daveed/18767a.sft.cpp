//remark:Microsoft and typeid of incomplete class types
//options:--microsoft_v=1911;fp:--microsoft_v=1912;fn

  #include <typeinfo>
  struct S;
  void f() {
    typeid(S);  // Previously a warning in all Microsoft C++ modes.
  }             // Now an error if microsoft_version >= 1912.

