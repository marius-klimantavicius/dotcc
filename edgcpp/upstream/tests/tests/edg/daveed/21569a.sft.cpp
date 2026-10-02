//remark:Microsoft mode nonreal instantiations and constexpr members
//options:--microsoft_v=1915 -tused;cp

  template<typename> struct V { static const int v = 42; };
  template<typename T> constexpr int v = V<T>::v;
  template<typename T> struct B {
    static constexpr int v = v<T>;  // Previously a spurious error.
  };
  template <typename T> struct D: B<T> {};
