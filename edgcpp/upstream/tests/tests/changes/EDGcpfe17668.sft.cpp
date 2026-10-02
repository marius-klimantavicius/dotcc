//type:fp
//options_all:--c++11
//remark:[4.13] C++11 constexpr folding and values returned by copy constructor
// 10/23/16 [EDGcpfe/17668,EDGcpfe/17504,EDGcpfe/17320]
//
// C++11 constexpr folding and values returned by copy constructor
//
// With constexpr functions in which copy elision is not performed and the
// value is returned by means of a constexpr copy constructor, the front end
// previously issued spurious "must have a constant value" diagnostics when the
// program was compiled in C++11 mode.  This is now fixed.
// --c++11:
struct S {
  constexpr S(const S &rhs) : val_(rhs.val_) { }
  constexpr S(int val) : val_(val) { }
  constexpr S operator|(const S &rhs) const {
    return S(val_ | rhs.val_);
  }
  int val_;
};

constexpr S Join() {
  return S(0);
}

template<class... Args>
constexpr S Join(const S &head, Args... tail) {
  return head | Join(tail...);
}

static constexpr S x = Join(1, 2, 4, 8);  // Previously not constant
