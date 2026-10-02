//remark:Dependent static data memnbers
//options:--c++11 --gnu_version  80100 -w;fp:--c++17;fp:--microsoft_v=1915;fp

  template<typename> struct S {
    constexpr S(int i) {}
    static constexpr S sdm = 0;
  };
