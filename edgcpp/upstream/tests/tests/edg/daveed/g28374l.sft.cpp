//remark:Clang __builtin_common_type
//options:--clang_v=210000 --c++20;fp:--clang_v=210000 --c++17;fp


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



struct const_ref_convertible {
 operator int&() const &;
 operator int&() && = delete;
};

#if __cplusplus >= 202002L
static_assert(__is_same(common_type_base<const_ref_convertible, int &>, type_identity<int>));
#else
static_assert(__is_same(common_type_base<const_ref_convertible, int &>, empty_type));
#endif
