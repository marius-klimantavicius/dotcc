//remark:Clang __builtin_common_type
//options:--clang_v=210000 --c++20;fp

struct empty_type {};

template <class T>
struct type_identity {
  using type = T;
};

template <class, class>
struct common_type;

template <class T1, class T2>
using common_type_t = typename common_type<T1, T2>::type;

template <class T1, class T2>
using common_type_base = __builtin_common_type<common_type_t, type_identity, empty_type, T1, T2>;

template <class T1, class T2>
struct common_type : common_type_base<T1, T2> {};

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


static_assert(__is_same(common_type_base<CommonTypeInt, CommonTypeInt>, type_identity<CommonTypeInt>));

