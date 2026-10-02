//type:fp
//options:--c++14 -w:--ms_c++14 -w

namespace in_class_scope
{
  struct C {
    static constexpr int V = 10;

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

  const int v = C::tgv<1> + C::ngv + C::tnv<1> + C::nnv;
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

  const int v = D<3>::C::tgv<1> + D<3>::C::ngv + D<3>::C::tnv<1> + D<3>::C::nnv;
}

namespace in_class_template_scope
{
  template<int H>
  struct C {
    static constexpr int V = 10;

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

  const int v =  C<3>::tgv<1> + C<3>::ngv + C<3>::tnv<1> + C<3>::nnv;
}

namespace in_class_template_in_class_template_scope
{
  template<int G>
  struct D
  {
    template<int H>
    struct C {
      static constexpr int V = 10;

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

  const int v = D<4>::C<3>::tgv<1> + D<4>::C<3>::ngv + D<4>::C<3>::tnv<1> +
                D<4>::C<3>::nnv;
}
