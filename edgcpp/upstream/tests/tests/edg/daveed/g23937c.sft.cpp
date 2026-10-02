//remark:Microsoft layout and explicit alignment
//options:--microsoft;fp

  #pragma pack(1)
  struct alignas(16) A {};
  struct B { A a; };
  struct D: B {};
  static_assert(alignof(D) == 16, "");

