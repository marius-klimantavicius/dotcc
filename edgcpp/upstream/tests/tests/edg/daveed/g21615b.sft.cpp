//remark:CTAD and missing initializers
//options:--c++17;fn

  template<typename ...> struct S { int x[2]; };
  constexpr S s;  // Deduces S<> as the type of s, but missing initializer
                  // was previously not diagnosed.  An error is now issued.

