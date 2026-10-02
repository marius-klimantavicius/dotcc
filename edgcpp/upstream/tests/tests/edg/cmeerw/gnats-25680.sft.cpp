//type:fp
//options:--c++11:--c++20
//options_all:--clang

namespace minimal
{
  struct C {
    char c1, c2, c3;
  };
  using AC = _Atomic C;
  static_assert(sizeof(AC) == 4 && alignof(AC) == 4, "Unexpected");
}

namespace complete_type_layout
{
  constexpr unsigned sizeof_largest_atomic = 2*sizeof(long);

  template<unsigned N, unsigned A = 1>
  struct alignas(A) char_array
  {
    char arr[N];
  };

  constexpr unsigned max(unsigned I, unsigned J)
  {
    return I >= J ? I : J;
  }

  constexpr unsigned expected_sizeof(unsigned N, unsigned A = 1)
  {
    return max((N > sizeof_largest_atomic) ? N :
               N <= 2 ? N :
               N <= 4 ? 4 :
               N <= 8 ? 8 : 16, A);
  }

  constexpr unsigned expected_alignof(unsigned N, unsigned A = 1)
  {
    return (N <= sizeof_largest_atomic) ? expected_sizeof(N, A) : A;
  }

  static_assert(sizeof(_Atomic(char_array<1>)) == expected_sizeof(1),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<1>)) == expected_alignof(1),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<1, 2>)) == expected_sizeof(1, 2),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<1, 2>)) == expected_alignof(1, 2),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<1, 4>)) == expected_sizeof(1, 4),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<1, 4>)) == expected_alignof(1, 4),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<1, 8>)) == expected_sizeof(1, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<1, 8>)) == expected_alignof(1, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<1, 16>)) == expected_sizeof(1, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<1, 16>)) == expected_alignof(1, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<2>)) == expected_sizeof(2),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<2>)) == expected_alignof(2),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<2, 2>)) == expected_sizeof(2, 2),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<2, 2>)) == expected_alignof(2, 2),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<2, 4>)) == expected_sizeof(2, 4),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<2, 4>)) == expected_alignof(2, 4),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<2, 8>)) == expected_sizeof(2, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<2, 8>)) == expected_alignof(2, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<2, 16>)) == expected_sizeof(2, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<2, 16>)) == expected_alignof(2, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<3>)) == expected_sizeof(3),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<3>)) == expected_alignof(3),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<3, 4>)) == expected_sizeof(3, 4),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<3, 4>)) == expected_alignof(3, 4),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<3, 8>)) == expected_sizeof(3, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<3, 8>)) == expected_alignof(3, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<3, 16>)) == expected_sizeof(3, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<3, 16>)) == expected_alignof(3, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<4>)) == expected_sizeof(4),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<4>)) == expected_alignof(4),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<4, 4>)) == expected_sizeof(4, 4),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<4, 4>)) == expected_alignof(4, 4),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<4, 8>)) == expected_sizeof(4, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<4, 8>)) == expected_alignof(4, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<4, 16>)) == expected_sizeof(4, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<4, 16>)) == expected_alignof(4, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<5>)) == expected_sizeof(5),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<5>)) == expected_alignof(5),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<5, 8>)) == expected_sizeof(5, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<5, 8>)) == expected_alignof(5, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<5, 16>)) == expected_sizeof(5, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<5, 16>)) == expected_alignof(5, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<6>)) == expected_sizeof(6, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<6>)) == expected_alignof(6, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<6, 8>)) == expected_sizeof(6, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<6, 8>)) == expected_alignof(6, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<6, 16>)) == expected_sizeof(6, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<6, 16>)) == expected_alignof(6, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<7>)) == expected_sizeof(7),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<7>)) == expected_alignof(7),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<7, 8>)) == expected_sizeof(7, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<7, 8>)) == expected_alignof(7, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<7, 16>)) == expected_sizeof(7, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<7, 16>)) == expected_alignof(7, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<8>)) == expected_sizeof(8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<8>)) == expected_alignof(8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<8, 8>)) == expected_sizeof(8, 8),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<8, 8>)) == expected_alignof(8, 8),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<8, 16>)) == expected_sizeof(8, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<8, 16>)) == expected_alignof(8, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<9>)) == expected_sizeof(9),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<9>)) == expected_alignof(9),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<9, 16>)) == expected_sizeof(9, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<9, 16>)) == expected_alignof(9, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<10>)) == expected_sizeof(10),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<10>)) == expected_alignof(10),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<10, 16>)) == expected_sizeof(10, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<10, 16>)) == expected_alignof(10, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<11>)) == expected_sizeof(11),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<11>)) == expected_alignof(11),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<11, 16>)) == expected_sizeof(11, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<11, 16>)) == expected_alignof(11, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<12>)) == expected_sizeof(12),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<12>)) == expected_alignof(12),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<12, 16>)) == expected_sizeof(12, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<12, 16>)) == expected_alignof(12, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<13>)) == expected_sizeof(13),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<13>)) == expected_alignof(13),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<13, 16>)) == expected_sizeof(13, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<13, 16>)) == expected_alignof(13, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<14>)) == expected_sizeof(14),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<14>)) == expected_alignof(14),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<14, 16>)) == expected_sizeof(14, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<14, 16>)) == expected_alignof(14, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<15>)) == expected_sizeof(15),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<15>)) == expected_alignof(15),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<15, 16>)) == expected_sizeof(15, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<15, 16>)) == expected_alignof(15, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<16>)) == expected_sizeof(16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<16>)) == expected_alignof(16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<16, 16>)) == expected_sizeof(16, 16),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<16, 16>)) == expected_alignof(16, 16),
                "alignof");

  static_assert(sizeof(_Atomic(char_array<17>)) == expected_sizeof(17),
                "sizeof");
  static_assert(alignof(_Atomic(char_array<17>)) == expected_alignof(17),
                "alignof");
}

namespace incomplete_type_layout
{
  struct C;

  // clang doesn't actually accept this
  using AC = _Atomic(C);

  struct C
  {
    char arr[3];
  };

  static_assert(sizeof(C) == 3, "sizeof");
  static_assert(alignof(C) == 1, "alignof");

  static_assert(sizeof(AC) == 4, "sizeof");
  static_assert(alignof(AC) == 4, "alignof");
}

namespace data_members
{
  struct C
  {
    char arr[3];
  };

  using AC = _Atomic(C);

  struct D
  {
    AC ac;
    char c;
  };

  static_assert(sizeof(C) == 3, "sizeof");
  static_assert(alignof(C) == 1, "alignof");

  static_assert(sizeof(AC) == 4, "sizeof");
  static_assert(alignof(AC) == 4, "alignof");

  static_assert(sizeof(D) == 8, "sizeof");
  static_assert(alignof(D) == 4, "alignof");
}

namespace overload_resolution
{
  struct C
  {
    char c1;
    char c2;
    char c3;
  };

  struct D : C
  {
    char c4;
  };

  using AC = _Atomic(C);

  struct X
  {
    operator AC() const;
  };

  static_assert(!__is_assignable(C, AC), "assignable");

  // this is the same as what clang does, but it differs from the assignable
  // concept below
  static_assert(!__is_assignable(AC, C), "assignable");

  static_assert(!__is_assignable(AC, D), "assignable");
  static_assert(!__is_assignable(AC, X), "assignable");

#if __cplusplus >= 202002L
  template<typename T1, typename T2>
  concept assignable = requires (T1 t1, T2 t2) { t1 = t2; };

  static_assert( assignable<C, C>);
  static_assert(!assignable<volatile C, C>);
  static_assert(!assignable<C, AC>);
  static_assert( assignable<AC, C>);
  static_assert( assignable<AC, volatile C>);
  static_assert(!assignable<AC, D>);
  static_assert(!assignable<AC, X>);

  template<typename T>
  void bar(T);

  template<typename T1, typename T2>
  concept callable = requires (T2 t2) { bar<T1>(t2); };

  static_assert(!callable<const C &, AC>);
#endif

  void foo(C c, AC ac)
  {
    ac = c;
    ac = ac;
  }
}
