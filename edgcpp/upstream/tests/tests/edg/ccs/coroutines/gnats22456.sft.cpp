//type:fp
//options::--clang_version 80000
//options_all:--c++17 --set_flag=coroutines -tused

namespace std {
inline namespace a {
template <bool, class b> using c = b;
template <int ab> struct e { static const int d = ab; };
template <bool g> using f = e<g>;
template <class> constexpr bool ap = f<0>::d;
template <class b> b &&forward();
template <class...> class t {
public:
  template <class h> t(h);
};
} // namespace a
namespace experimental {
inline namespace i {
template <typename j, typename> struct coroutine_traits : j {};
template <typename = void> class coroutine_handle;
template <> class coroutine_handle<> {};
template <typename k> class coroutine_handle : public coroutine_handle<> {
public:
  static coroutine_handle from_address(void *);
  static coroutine_handle l(k);
};
struct m {
  bool await_ready() noexcept;
  void await_suspend(coroutine_handle<>) noexcept;
  void await_resume() noexcept;
};
} // namespace i
} // namespace experimental
} // namespace std
class o {
  using n = std::experimental::coroutine_handle<o>;

public:
  auto get_return_object() { return n::l(*this); }
  std::experimental::m initial_suspend();
  auto final_suspend() noexcept {
    class D {
    public:
      bool await_ready() noexcept;
      void await_suspend(n) noexcept;
      void await_resume() noexcept;
    };
    return D{};
  }
  void return_void();
  void unhandled_exception();
};
class v {
public:
  using promise_type = o;
  using n = std::experimental::coroutine_handle<>;
  v(n);
};
template <typename ad, std::c<std::ap<int>, int> = 0> v p(ad) {
  co_await std::forward<ad>();
  co_return;
}
template <typename ad> void q(ad) { p(std::forward<ad>()); }
class r {
public:
  r(std::t<>);
  auto operator co_await() {
    struct ae {
      bool await_ready();
      bool await_suspend(std::experimental::coroutine_handle<>);
      std::t<> await_resume();
      r s;
    };
    return ae{*this};
  }
};
auto u() {
  r af(0);
  q(af);
}
