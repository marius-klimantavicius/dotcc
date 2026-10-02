//type:fp
//options_all:--microsoft
//remark:[6.4] Microsoft C++ compatibility: Triviality of deleted default constructor
// 9/15/22  [EDGcpfe/25523]
//
// Microsoft C++ compatibility: Triviality of deleted default constructor
//
// In Microsoft mode, a deleted default constructor is never treated as "trivial".
struct D {
  D() = delete;
  D(int);
};
static_assert(!__has_trivial_constructor(D), "Standard");
  // Now accepted in Microsoft mode
