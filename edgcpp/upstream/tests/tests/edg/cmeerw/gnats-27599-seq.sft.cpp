//type:fp
//options:--ms_c++20 --microsoft_version 1936:--c++20 --clang_version 190100
template<typename ... Ts>
constexpr bool f(Ts ...) { return true; }

template<typename, unsigned int>
struct D
{
  static constexpr bool v = true;
};

template <class _Ty, _Ty... _Vals>
struct integer_sequence
{
  using value_type = _Ty;
};

template<typename ... Ts>
struct C
{ };

template<typename T1, typename ... Ts>
struct C<T1, Ts ...>
{
  static bool not_const();

  template<bool B, class U,
      class I = __make_integer_seq<integer_sequence, unsigned int, sizeof ... (Ts)> >
  static constexpr bool v = false;

  template<typename U, unsigned int... I>
  static constexpr bool v<false, U, integer_sequence<unsigned int, I...>> =
    f(D<Ts, I>::v ...);

  template<typename U> requires v<false, U>
  void g(U);
};

int main()
{
  C<char, short, int> c;
  c.g(C<char, short, int>());
}
