//type:cp
//options:--gnu_version 70000
//options_all:--c++17 -tused

template <typename _Tp, _Tp __v> struct A { static constexpr _Tp value = __v; };
template <typename _Tp> _Tp declval();
template <typename _From, typename _To> struct B {
  template <typename _To1> static void __test_aux(_To1);
  template <typename _From1, typename,
    typename = decltype(__test_aux(declval<_From1>()))>
    static A<bool, true> __test(int);
  template <typename, typename> static A<bool, false> __test(...);
  typedef decltype(__test<_From, _To>(0)) type;
};
template <typename _From> struct F : B<_From, int>::type {};
template <bool, typename> struct C;
template <typename _Tp> struct C<true, _Tp> { typedef _Tp type; };
template <bool _Cond, typename _Tp>
  using enable_if_t = typename C<_Cond, _Tp>::type;
template <typename _Tp> _Tp forward(_Tp &);
struct D {
  D(D &&);
};
template <typename...> class G : D {
 public:
  G(G &&) noexcept(false) = default;
};
template <typename> class optional {
 public:
  template <typename _Up, enable_if_t<F<_Up &&>::value, bool> = true>
    optional(_Up);
};
template <typename _Tp> void make_optional(_Tp &&__t) {
  optional<_Tp>{forward(__t)};
}
using Entry = G<>;
void getEntry(Entry entry) { make_optional(entry); }
