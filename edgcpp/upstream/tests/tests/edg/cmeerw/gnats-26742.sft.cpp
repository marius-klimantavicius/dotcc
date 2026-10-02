//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T, typename U>
  concept C = sizeof(T) == sizeof(U);
  template<typename ... T>
  struct B {
    template<C<T> ... U>
    static int f(U ...);
  };
  auto v = B<char, int>::f('a', 2);
}

namespace pack_expansions
{
  template<typename T>
  concept C1 = true;

  template<typename T, typename U>
  concept C2 = true;

  template<int>
  struct DN
  { };

  template<typename>
  struct DT
  { };

  template<typename ... Ts>
  struct B
  {
    template<Ts ... X>
    static void f(DN<X> ...);

    template<C2<Ts> ... XT>
    static void g(DT<XT> ...);

    template<C2<Ts> ... XT>
    static void g(XT ...);

    template<C1 ... XT>
    static void h(DT<XT> ...);
  };

  auto f = (B<int, char>::f<1, 'a'>(DN<1>(), DN<'a'>()), 1);
  auto g1 = (B<int, char>::g<int, char>(DT<int>(), DT<char>()), 1);
  auto g2 = (B<int, char>::g<int, char>(1, 'a'), 1);
  auto h = (B<int, char>::h<int, char>(DT<int>(), DT<char>()), 1);
}

namespace expand_type_constraint
{
  template<typename T, typename U>
  concept C = sizeof(T) == sizeof(U);

  template<typename>
  struct D
  { D(int); };

  template<typename ... Ts>
  struct B
  {
    template<C<Ts> ... XT>
    static int g(D<XT> ...);
  };

  auto v0 = B<>::g<>();
  auto v1e = B<int>::g<int>(1);
  auto v1i = B<int>::g(D<int>(1));
  auto v2e = B<int, char>::g<int, char>(1, 'a');
  auto v2i = B<int, char>::g(D<int>(1), D<char>('a'));
  auto v2i1 = B<int, char>::g<int>(D<int>(1), D<char>('a'));
  auto v3e = B<int, char, short>::g<int, char, short>(1, 'a', 2);
  auto v3i = B<int, char, short>::g(D<int>(1), D<char>('a'), D<short>(2));
}

namespace type_constraint_pack_containing_expansion
{
  template<typename ... T>
  concept C = true;

  template<typename>
  struct D
  { D(int); };

  template<typename ... Ts>
  struct B
  {
    template<C<Ts ...> ... XT>
    static int g(D<XT> ...);
  };

  auto v0 = B<>::g<>();
  auto v1e = B<int>::g<int>(1);
  auto v1i = B<int>::g(D<int>(1));
  auto v21e = B<int, char>::g<int>(1);
  auto v21i = B<int, char>::g(D<int>(1));
  auto v12e = B<int>::g<int, char>(1, 'a');
  auto v12i = B<int>::g(D<int>(1), D<char>('a'));
  auto v2e = B<int, char>::g<int, char>(1, 'a');
  auto v2i = B<int, char>::g(D<int>(1), D<char>('a'));
  auto v2i1 = B<int, char>::g<int>(D<int>(1), D<char>('a'));
  auto v3e = B<int, char, short>::g<int, char, short>(1, 'a', 2);
  auto v3i = B<int, char, short>::g(D<int>(1), D<char>('a'), D<short>(2));
}
