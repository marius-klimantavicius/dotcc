//type:fp
//remark:[4.12] Incomplete return type in call not handled during SFINAE processing
// 6/3/16   [EDGcpfe/17040]
//
// Incomplete return type in call not handled during SFINAE processing
//
// The front end sometimes diagnosed a call to a function with an incomplete
// return type directly during SFINAE processing instead of discarding the
// associated candidate function.
//
// This is now fixed.
struct I f();  // Incomplete return type.
template<typename T> int g(decltype(f(T{}), 0));  // (1)
template<typename T> char g(...);
static_assert(sizeof(g<int>(42)) == 1, "Unexpected!");
  // Previously triggered an "incomplete return type" error while
  // considering candidate (1).
