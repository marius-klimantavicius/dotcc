//type:fp
//options:--c++20 --clang_version 160000
//options_all:-w

template<typename T1, typename T2>
struct is_same
{
  static constexpr bool value = false;
};

template<typename T>
struct is_same<T, T>
{
  static constexpr bool value = true;
};

template<bool>
struct is_true
{ };

template<>
struct is_true<true>
{
  static constexpr bool value = true;
};


template<typename T, typename U>
concept underlying_type = requires {
  is_true<is_same<U, __underlying_type(T)>::value>::value;
};

enum E : short { };
static_assert(underlying_type<E, short>);
static_assert(!underlying_type<E, void>);
static_assert(!underlying_type<E, E>);


template<typename T, typename U>
concept add_lvalue_reference = requires {
  is_true<is_same<U, __add_lvalue_reference(T)>::value>::value;
};

static_assert(add_lvalue_reference<int, int &>);
static_assert(add_lvalue_reference<int &&, int &>);
static_assert(!add_lvalue_reference<int, int>);


template<typename T, typename U>
concept add_rvalue_reference = requires {
  is_true<is_same<U, __add_rvalue_reference(T)>::value>::value;
};

static_assert(add_rvalue_reference<int, int &&>);
static_assert(add_rvalue_reference<int &&, int &&>);
static_assert(!add_rvalue_reference<int, int>);


template<typename T, typename U>
concept add_pointer = requires {
  is_true<is_same<U, __add_pointer(T)>::value>::value;
};

static_assert(add_pointer<int, int *>);
static_assert(!add_pointer<int, int>);


template<typename T, typename U>
concept decay = requires {
  is_true<is_same<U, __decay(T)>::value>::value;
};

static_assert(decay<int[], int *>);
static_assert(!decay<int[], int[]>);


template<typename T, typename U>
concept make_signed = requires {
  is_true<is_same<U, __make_signed(T)>::value>::value;
};

static_assert(make_signed<unsigned short, signed short>);
static_assert(!make_signed<unsigned short, unsigned short>);


template<typename T, typename U>
concept make_unsigned = requires {
  is_true<is_same<U, __make_unsigned(T)>::value>::value;
};

static_assert(make_unsigned<signed short, unsigned short>);
static_assert(!make_unsigned<signed short, signed short>);


template<typename T, typename U>
concept remove_all_extents = requires {
  is_true<is_same<U, __remove_all_extents(T)>::value>::value;
};

static_assert(remove_all_extents<int[2][3], int>);
static_assert(!remove_all_extents<int[2][3], int[2][3]>);


template<typename T, typename U>
concept remove_const = requires {
  is_true<is_same<U, __remove_const(T)>::value>::value;
};

static_assert(remove_const<const volatile int, volatile int>);
static_assert(!remove_const<const volatile int, const volatile int>);


template<typename T, typename U>
concept remove_cv = requires {
  is_true<is_same<U, __remove_cv(T)>::value>::value;
};

static_assert(remove_cv<const volatile int, int>);
static_assert(!remove_cv<const volatile int, const volatile int>);


template<typename T, typename U>
concept remove_cvref = requires {
  is_true<is_same<U, __remove_cvref(T)>::value>::value;
};

static_assert(remove_cvref<const int &, int>);
static_assert(!remove_cvref<const int &, const int &>);


template<typename T, typename U>
concept remove_extent = requires {
  is_true<is_same<U, __remove_extent(T)>::value>::value;
};

static_assert(remove_extent<int[2][3], int[3]>);
static_assert(!remove_extent<int[2][3], int[2][3]>);


template<typename T, typename U>
concept remove_pointer = requires {
  is_true<is_same<U, __remove_pointer(T)>::value>::value;
};

static_assert(remove_pointer<int *, int>);
static_assert(!remove_pointer<int *, int *>);


template<typename T, typename U>
concept remove_reference_t = requires {
  is_true<is_same<U, __remove_reference_t(T)>::value>::value;
};

static_assert(remove_reference_t<int &, int>);
static_assert(!remove_reference_t<int &, int &>);


template<typename T, typename U>
concept remove_restrict = requires {
  is_true<is_same<U, __remove_restrict(T)>::value>::value;
};

static_assert(remove_restrict<int *, int *>);
static_assert(!remove_restrict<int *, int>);


template<typename T, typename U>
concept remove_volatile = requires {
  is_true<is_same<U, __remove_volatile(T)>::value>::value;
};

static_assert(remove_volatile<const volatile int, const int>);
static_assert(!remove_volatile<const volatile int, const volatile int>);


template<typename T, typename E>
void f()
{
  is_true<is_same<short, __underlying_type(E)>::value>::value;
  is_true<is_same<T &, __add_lvalue_reference(T)>::value>::value;
  is_true<is_same<T &&, __add_rvalue_reference(T)>::value>::value;
  is_true<is_same<T *, __add_pointer(T)>::value>::value;
  is_true<is_same<T *, __decay(T[])>::value>::value;
  is_true<is_same<T, __make_signed(T)>::value>::value;
  is_true<is_same<unsigned int, __make_unsigned(T)>::value>::value;
  is_true<is_same<T, __remove_all_extents(T[2][3])>::value>::value;
  is_true<is_same<volatile T, __remove_const(const volatile T)>::value>::value;
  is_true<is_same<T, __remove_cv(const volatile T)>::value>::value;
  is_true<is_same<T, __remove_cvref(const T &)>::value>::value;
  is_true<is_same<T[3], __remove_extent(T[2][3])>::value>::value;
  is_true<is_same<T, __remove_pointer(T *)>::value>::value;
  is_true<is_same<T, __remove_reference_t(T &)>::value>::value;
  is_true<is_same<T *, __remove_restrict(T *)>::value>::value;
  is_true<is_same<const T, __remove_volatile(const volatile T)>::value>::value;
}

template void f<int, E>();
