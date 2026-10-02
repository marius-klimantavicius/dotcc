//type:fp
//options_all:-w --gn 120100 --c++20
//remark:[6.8] Constant-evaluation of std::construct_at for a union
// 7/2/25   [EDGcpfe/28207]
//
// Constant-evaluation of std::construct_at for a union
//
// The use of std::construct_at (a type-safe interface to placement-new that can
// be evaluated at compile time) for a union type previously caused the active
// member of the result of that operation to be de-activated.  In the example
// above, that resulted in the static_assert condition "g() == 2" spuriously being
// reported as non-constant.  This is a regression introduced in version 6.7 of
// the front end by the changes for EDGcpfe/27588.  That is now fixed.
inline void *operator new(decltype(sizeof(1)), void *ptr) noexcept {
  return ptr;
}
void operator delete(void*, void*);
namespace std {
  // A simplified implementation of std::construct_at sufficient to
  // illustrate the issue.
  template<typename T, typename... Args>
    constexpr T* construct_at(T *ptr, Args &&...args) noexcept {
      return ::new((void *)ptr) T(args...);
    }
}
union U {
  int x;
  constexpr U(int x): x(x) {}
};
constexpr int g() {
  U u(1);
  std::construct_at(&u, 2);
  return u.x;
}
static_assert(g() == 2);  // Previously failed.  Now okay.
