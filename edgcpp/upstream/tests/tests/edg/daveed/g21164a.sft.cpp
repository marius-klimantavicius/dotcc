//remark:__reference_binds_to_temporary
//options:--clang_v=70000 -w;fp

#define F(x) (x ? -1 : 1)
#define T(x) (x ? 1 : -1)

template <class T, class RefType = T &>
struct ConvertsToRef {
  operator RefType() const { return static_cast<RefType>(obj); }
  mutable T obj = 42;
};

void reference_binds_to_temporary_checks() {
  { int arr[F((__reference_binds_to_temporary(int &, int &)))]; }
  { int arr[F((__reference_binds_to_temporary(int &, int &&)))]; }

  { int arr[F((__reference_binds_to_temporary(int const &, int &)))]; }
  { int arr[F((__reference_binds_to_temporary(int const &, int const &)))]; }
  { int arr[F((__reference_binds_to_temporary(int const &, int &&)))]; }

  { int arr[F((__reference_binds_to_temporary(int &, long &)))]; } // doesn't construct
  { int arr[T((__reference_binds_to_temporary(int const &, long &)))]; }
  { int arr[T((__reference_binds_to_temporary(int const &, long &&)))]; }
  { int arr[T((__reference_binds_to_temporary(int &&, long &)))]; }

  using LRef = ConvertsToRef<int, int &>;
  using RRef = ConvertsToRef<int, int &&>;
  using CLRef = ConvertsToRef<int, const int &>;
  using LongRef = ConvertsToRef<long, long &>;
  { int arr[T((__is_constructible(int &, LRef)))]; }
  { int arr[F((__reference_binds_to_temporary(int &, LRef)))]; }

  { int arr[T((__is_constructible(int &&, RRef)))]; }
  { int arr[F((__reference_binds_to_temporary(int &&, RRef)))]; }

  { int arr[T((__is_constructible(int const &, CLRef)))]; }
  { int arr[F((__reference_binds_to_temporary(int &&, CLRef)))]; }

  { int arr[T((__is_constructible(int const &, LongRef)))]; }
  { int arr[T((__reference_binds_to_temporary(int const &, LongRef)))]; }

  // Test that it doesn't accept non-reference types as input.
  { int arr[F((__reference_binds_to_temporary(int, long)))]; }

  { int arr[T((__reference_binds_to_temporary(const int &, long)))]; }
}
