//remark:Inline const static data members
//options:--c++17;fn

  struct S {
    static inline int const ic;  // Previously undiagnosed.  Now an error
  };                             // (missing initializer).

