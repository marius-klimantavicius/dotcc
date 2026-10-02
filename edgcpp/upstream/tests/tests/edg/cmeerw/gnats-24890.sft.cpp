//type:fp
//options:--c++17:--c++20:--c++17 --g++

namespace minimal
{
  struct C {
    template<int I>
    static inline auto l = [] (auto j) {
      return I + j;  // previously: identifier "I" is undefined
    };
  };
  auto v = C::l<1>(1);
}

namespace in_class_scope
{
  struct C
  {
    static constexpr int V = 10;

    template<int I>
    static constexpr auto tgl = [] (auto j) { return I + j + V; };

    static constexpr auto ngl = [] (auto j) { return j + V; };

    template<int I>
    static constexpr auto tnl = [] (int j) { return I + j + V; };

    static constexpr auto nnl = [] (int j) { return j + V; };

    template<int I>
    static const int tgv;

    static const int ngv;

    template<int I>
    static const int tnv;

    static const int nnv;
  };

  template<int I>
  const int C::tgv = [] (auto j) { return I + j + V; } (2);

  const int C::ngv = [] (auto j) { return j + V; } (2);

  template<int I>
  const int C::tnv = [] (int j) { return I + j + V; } (2);

  const int C::nnv = [] (int j) { return j + V; } (2);

  static_assert(C::tgl<1>(2) == 13);
  static_assert(C::ngl(2) == 12);
  static_assert(C::tnl<1>(2) == 13);
  static_assert(C::nnl(2) == 12);

  static_assert(C::tgv<1> == 13);
  static_assert(C::ngv == 12);
  static_assert(C::tnv<1> == 13);
  static_assert(C::nnv == 12);
}

namespace in_class_in_class_template_scope
{
  template<int H>
  struct D
  {
    struct C
    {
      static constexpr int V = 10;

      template<int I>
      static constexpr auto tgl = [] (auto j) { return H + I + j + V; };

      static constexpr auto ngl = [] (auto j) { return H + j + V; };

      template<int I>
      static constexpr auto tnl = [] (int j) { return H + I + j + V; };

      static constexpr auto nnl = [] (int j) { return H + j + V; };

      template<int I>
      static const int tgv;

      static const int ngv;

      template<int I>
      static const int tnv;

      static const int nnv;
    };
  };

  template<int H>
  template<int I>
  const int D<H>::C::tgv = [] (auto j) { return H + I + j + V; } (2);

  template<int H>
  const int D<H>::C::ngv = [] (auto j) { return H + j + V; } (2);

  template<int H>
  template<int I>
  const int D<H>::C::tnv = [] (int j) { return H + I + j + V; } (2);

  template<int H>
  const int D<H>::C::nnv = [] (int j) { return H + j + V; } (2);

  static_assert(D<3>::C::tgl<1>(2) == 16);
  static_assert(D<3>::C::ngl(2) == 15);
  static_assert(D<3>::C::tnl<1>(2) == 16);
  static_assert(D<3>::C::nnl(2) == 15);

  static_assert(D<3>::C::tgv<1> == 16);
  static_assert(D<3>::C::ngv == 15);
  static_assert(D<3>::C::tnv<1> == 16);
  static_assert(D<3>::C::nnv == 15);
}

namespace in_namespace_scope
{
  namespace ns
  {
    static constexpr int V = 10;

    template<int I>
    static constexpr auto tgl = [] (auto j) { return I + j + V; };

    static constexpr auto ngl = [] (auto j) { return j + V; };

    template<int I>
    static constexpr auto tnl = [] (int j) { return I + j + V; };

    static constexpr auto nnl = [] (int j) { return j + V; };
  };

  static_assert(ns::tgl<1>(2) == 13);
  static_assert(ns::ngl(1) == 11);
  static_assert(ns::tnl<1>(1) == 12);
  static_assert(ns::nnl(1) == 11);
}

namespace in_class_template_scope
{
  template<int H>
  struct C
  {
    static constexpr int V = 10;

    template<int I>
    static constexpr auto tgl = [] (auto j) { return H + I + j + V; };

    static constexpr auto ngl = [] (auto j) { return H + j + V; };

    template<int I>
    static constexpr auto tnl = [] (int j) { return H + I + j + V; };

    static constexpr auto nnl = [] (int j) { return H + j + V; };

    template<int I>
    static const int tgv;

    static const int ngv;

    template<int I>
    static const int tnv;

    static const int nnv;
  };

  template<int H>
  template<int I>
  const int C<H>::tgv = [] (auto j) { return H + I + j + V; } (2);

  template<int H>
  const int C<H>::ngv = [] (auto j) { return H + j + V; } (2);

  template<int H>
  template<int I>
  const int C<H>::tnv = [] (int j) { return H + I + j + V; } (2);

  template<int H>
  const int C<H>::nnv = [] (int j) { return H + j + V; } (2);

  static_assert(C<3>::tgl<1>(2) == 16);
  static_assert(C<3>::ngl(2) == 15);
  static_assert(C<3>::tnl<1>(2) == 16);
  static_assert(C<3>::nnl(2) == 15);

  static_assert(C<3>::tgv<1> == 16);
  static_assert(C<3>::ngv == 15);
  static_assert(C<3>::tnv<1> == 16);
  static_assert(C<3>::nnv == 15);
}

namespace in_class_template_in_class_template_scope
{
  template<int G>
  struct D
  {
    template<int H>
    struct C
    {
      static constexpr int V = 10;

      template<int I>
      static constexpr auto tgl = [] (auto j) { return G + H + I + j + V; };

      static constexpr auto ngl = [] (auto j) { return G + H + j + V; };

      template<int I>
      static constexpr auto tnl = [] (int j) { return G + H + I + j + V; };

      static constexpr auto nnl = [] (int j) { return G + H + j + V; };

      template<int I>
      static const int tgv;

      static const int ngv;

      template<int I>
      static const int tnv;

      static const int nnv;
    };
  };

  template<int G>
  template<int H>
  template<int I>
  const int D<G>::C<H>::tgv = [] (auto j) { return G + H + I + j + V; } (2);

  template<int G>
  template<int H>
  const int D<G>::C<H>::ngv = [] (auto j) { return G + H + j + V; } (2);

  template<int G>
  template<int H>
  template<int I>
  const int D<G>::C<H>::tnv = [] (int j) { return G + H + I + j + V; } (2);

  template<int G>
  template<int H>
  const int D<G>::C<H>::nnv = [] (int j) { return G + H + j + V; } (2);

  static_assert(D<4>::C<3>::tgl<1>(2) == 20);
  static_assert(D<4>::C<3>::ngl(2) == 19);
  static_assert(D<4>::C<3>::tnl<1>(2) == 20);
  static_assert(D<4>::C<3>::nnl(2) == 19);

  static_assert(D<4>::C<3>::tgv<1> == 20);
  static_assert(D<4>::C<3>::ngv == 19);
  static_assert(D<4>::C<3>::tnv<1> == 20);
  static_assert(D<4>::C<3>::nnv == 19);
}
