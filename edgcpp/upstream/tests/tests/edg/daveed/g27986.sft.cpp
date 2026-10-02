//remark:Overload resolution quirk of GCC
//options:--gnu=120101 --c++20;fp:--clang_v=150000 --c++20;fp:--c++20;fp

template<typename T1, typename T2> bool operator==(T1 const& , T2 const&);
  struct S {
    template <typename > void operator==(S);
    template <typename U = int> int operator==(S const&) const;
    template <typename U = int> int operator!=(S const&) const;
  };
  S f();
  auto r = f() != f();  // Previously an error in GNU and Clang C++20 modes.

