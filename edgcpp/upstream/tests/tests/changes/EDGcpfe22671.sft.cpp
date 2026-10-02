//type:fp
//options_all:--gn 70300
//remark:[6.1] Spurious "incomplete type is not allowed" in friend template declaration
// 4/28/20  [EDGcpfe/22671]
//
// Spurious "incomplete type is not allowed" in friend template declaration
//
// In some situations, the front end issued a spurious "incomplete type is not
// allowed" diagnostic while parsing a friend template declaration.
//
// That regression, introduced in version 5.0 by the changes for EDGcpfe/19506,
// is now fixed.
template<typename> void g();
template<unsigned I, typename T>
  auto h(T &&p) -> decltype(g<I>(static_cast<T&&>(p)));
template<typename T> struct X : T {
  template<unsigned I, typename U >
    friend auto f(X<T> &&p) -> decltype(h<I>(static_cast<U&&>(p)));
        // Previously complained about p having an incomplete type in
};      // the static_cast expression.  Now accepted.
