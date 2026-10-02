//type:fn
//options_all:--c++17
//remark:[5.1] Out-of-class declarations of inline static data members
// 4/4/19   [EDGcpfe/20978]
//
// Out-of-class declarations of inline static data members
//
// The front end previously failed to diagnose out-of-class declarations of
// non-constexpr inline static data members.  Such redundant declarations are
// permitted for constexpr inline static data members, which are implicitly
// inline, but not for those that are inline but not constexpr.  This is now
// fixed.
struct S {
  static inline const int i = 0;
};
const int S::i;   // Previously erroneously accepted, now an error
