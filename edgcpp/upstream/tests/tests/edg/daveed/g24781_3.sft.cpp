//remark:Explicit-this member functions
//options:--c++23;fp:--c++23 -DNEG;fn

template <typename T> struct remove_reference { using type = T; };
template <typename T> struct remove_reference<T&> { using type = T; };
template <typename T> struct remove_reference<T&&> { using type = T; };
template <typename T> using remove_reference_t = typename remove_reference<T>::type;

template <typename T> struct remove_const { using type = T; };
template <typename T> struct remove_const<T const> { using type = T; };
template <typename T> using remove_const_t = typename remove_const<T>::type;

template <typename T> using decay_t = remove_reference_t<remove_const_t<T>>;

template <typename T, typename U>
inline constexpr bool is_same_v = false;
template <typename T>
inline constexpr bool is_same_v<T, T> = true;

template <typename F>
struct call_wrapper {
  F f;

  template <typename Self, typename... Args>
  auto operator()(this Self&& self, Args&&... args)
	-> decltype(((Self&&)self).f((Args&&)args...))
  {
	return ((Self&&)self).f((Args&&)args...);
  }
};

template <typename F>
auto not_fn(F&& f) {
  return call_wrapper<decay_t<F>>{(F&&)f};
}

struct unfriendly {
    template <typename T>
    auto operator()(T v) {
        static_assert(is_same_v<T, int>);
        return v;
    }

    template <typename T>
    auto operator()(T v) const {
        static_assert(is_same_v<T, double>);
        return v;
    }
};

struct fun {
    template <typename... Args>
    void operator()(Args&&...) = delete;

    template <typename... Args>
    bool operator()(Args&&...) const { return true; }
};

void check() {
	not_fn(unfriendly{})(1); // yay
#ifdef NEG
	not_fn(fun{})();         // EXPECT: error
#endif
}
