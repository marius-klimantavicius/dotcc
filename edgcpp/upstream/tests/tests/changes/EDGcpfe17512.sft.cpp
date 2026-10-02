//type:fp
//options_all:--microsoft_version=1900
//remark:[4.12] Reused temporary values in constant expressions
// 9/12/16  [EDGcpfe/17512]
//
// Reused temporary values in constant expressions
//
// The front end previously reported a spurious "must have a constant value"
// error when a reused temporary value appears in a context requiring a
// constant expression.  (Such reused temporaries arise in uses of initializer
// lists, Microsoft properties, and the GNU abbreviated conditional operator.)
// This is now fixed.
namespace std {
template<class _Elem> struct initializer_list {
  constexpr initializer_list(const _Elem *_First_arg,
                             const _Elem *_Last_arg) noexcept
        : _First(_First_arg), _Last(_Last_arg) { }
  constexpr const _Elem *begin() const noexcept {
    return (_First);
  }
  const _Elem *_First;
  const _Elem *_Last;
};
}
struct Y {
  int i;
  int j;
  constexpr Y(std::initializer_list<int> il)
        : i(il.begin()[0]), j(il.begin()[1]) { }
};
int main() {
  constexpr Y y{3, 1};  // Previously a spurious error
}
