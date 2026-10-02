//type:fp
//options:--c++20:--ms_c++20

namespace PR_test
{
  template <class TC> constexpr int d = 42;
  struct m {
    template <template <class... TD> class TE> using i = TE<>;
  };
  template <template <class... TF> class TG> using h = m::i<TG>;
  template <class TH> using j = h<TH::template k>;
  template <bool TM> struct B;
  template <bool TN, class TO, class TP> using r = j<B<TN>>;
  template <class UC, class... UD> concept ak = requires(UC al, UD... am) {
    an(al, am...);
  };
  struct ap;
  template <class UG, class UH, class UI> using ay = j<decltype((UG) nullptr)>;
  template <class UJ, class UK> concept bd = ak<ap, UJ, UK>;
  template <class UL> concept be = true;
  struct ap {
    template <class UM, class UN = int>
    requires(bd<UM, UN> || be<UN>) void operator()(UM, UN) {}
  };
  ap bf;
  template <class UO> concept bg = requires(UO bh) { bf(bh, {}); };
  template <class UP, class UQ> using bj = UQ;
  template <class UT, class UV, class UW, class UX, class UY>
  using bm = ay<bj<UV, UW>, int, int>;
  template <class UZ, class VA> using bo = int;
  template <class VC, class VD, class VE, class VF>
  using bt = r<d<bm<VC, VE, VF, int, int>>, int, int>;
  struct bqq {
    template <class VH, class VI>
    using bz = bo<bt<int, int, int, VI>, int>;
    template <class VJ, class VK> friend bz<VJ, VK> an(ap, VJ, VK){ return {}; }
  };
  template <class VP, class VQ> requires bg<bqq> void ff(VP, VQ);
  void main() {
    ff(1, 2);
  }
}

namespace simple_test
{
  template<typename T>
  struct C
  {
    template<typename U>
    struct k { };
  };

  template<class TC>
  constexpr bool b = true;

  template<bool TM>
  using B = int;

  template <class M1>
  using bm = typename decltype(C<M1>())::template k<int>;

  template <class T1, class T2>
  using bt = B<b<bm<T2>>>;

  template <class T1>
  using bz = B<b<bm<T1>>>;

  template<class VK> bz<VK> an(VK);

  void foo() {
    an<int>(1);
  }
}

namespace type_equivalence
{
  template<class T>
  void f(typename decltype(T::A)::template N<int>)
  { }

  template<class T>
  void f(typename decltype(T::A)::template N<long>)
  { }

  template<class T>
  void f(typename decltype(T::B)::template N<int>)
  { }

  struct C
  {
    template<class T>
    void f(typename decltype(T::A)::template N<int>);

    template<class T>
    void f(typename decltype(T::B)::template N<int>);
  };

  template<class T>
  void C::f(typename decltype(T::A)::template N<int>)
  { }

  template<class T>
  void C::f(typename decltype(T::B)::template N<int>)
  { }
}
