//type:fp
//options_all:--g++ --c++11
//remark:[4.10.1] Dependent expressions in constant contexts
// 2/15/15   [EDGcpfe/15612]
//
// Dependent expressions in constant contexts
//
// The front end previously issued a spurious "must have a constant value"
// error for some expressions involving dependent values in template
// definitions.  This is now fixed.
enum Test {
  E = 0
};

constexpr Test operator&(const Test lhs, const Test rhs) {
  return static_cast<Test>(static_cast<int>(lhs) & static_cast<int>(rhs));
}

template<Test TEST> struct C {
  static const Test value = TEST & Test::E;  // Previously an error
};
