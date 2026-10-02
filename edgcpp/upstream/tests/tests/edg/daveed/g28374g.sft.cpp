//remark:Clang __builtin_common_type
//options:--clang_v=210000 --c++20;fp

struct empty_type {};

template <class T>
struct type_identity {
  using type = T;
};

template <class...>
struct common_type;

template <class... Args>
using common_type_t = typename common_type<Args...>::type;

template <class... Args>
using common_type_base = __builtin_common_type<common_type_t, type_identity, empty_type, Args...>;

template <class... Args>
struct common_type : common_type_base<Args...> {};

struct NoCommonType {};

template <>
struct common_type<NoCommonType, NoCommonType> {};

struct CommonTypeInt {};

template <>
struct common_type<CommonTypeInt, CommonTypeInt> {
  using type = int;
};

template <>
struct common_type<CommonTypeInt, int> {
  using type = int;
};

template <>
struct common_type<int, CommonTypeInt> {
  using type = int;
};

static_assert(__is_same(common_type_base<CommonTypeInt&, CommonTypeInt&&>, type_identity<int>));
static_assert(__is_same(common_type_base<CommonTypeInt>, type_identity<int>));
static_assert(__is_same(common_type_base<NoCommonType, NoCommonType, NoCommonType>, empty_type));
static_assert(__is_same(common_type_base<CommonTypeInt, CommonTypeInt, CommonTypeInt>, type_identity<int>));
static_assert(__is_same(common_type_base<NoCommonType>, empty_type));

