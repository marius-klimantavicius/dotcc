//type:fp
//options:--c++20 -A;fn:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<typename>
  struct C {
    C(int);
  };
  C(auto i) -> C<decltype(i)>;
}

namespace non_dpdt
{
  template<typename>
  struct C
  {
    C(int);
  };

  C(int i) -> C<int>;
  C c{1};
}

namespace abbreviated_function
{
  template<typename>
  struct C
  {
    C(int);
  };

  C(auto i) -> C<int>;
  C c{1};
}

namespace non_dpdt_param
{
  template<typename>
  struct C
  {
    C(int);
  };

  C(int i) -> C<decltype(i)>;
  C c{1};
}

namespace dpdt_param
{
  template<typename>
  struct C
  {
    C(int);
  };

  C(auto i) -> C<decltype(i)>;
  C c{1};
}

namespace disambig
{
  template<typename T>
  struct C
  {
    C(T);
  };

  C (c1)(1);
}

namespace constrained_auto
{
  template<typename T>
  concept X = true;

  template<typename T>
  struct C
  {
    C(T);
  };

  C(X auto x) -> C<decltype(x)>;

  C c{1};
}

namespace disambig_constrained_auto
{
  template<typename> concept X = true;
  void f(int(*)(), X auto);
}
