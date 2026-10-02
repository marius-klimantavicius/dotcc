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

struct Incomplete;

template<>
struct common_type<Incomplete, Incomplete>;

struct S {};
struct T : S {};

static_assert(__is_same(common_type_base<int S::*, int S::*>, type_identity<int S::*>));
static_assert(__is_same(common_type_base<int S::*, int T::*>, type_identity<int T::*>));
static_assert(__is_same(common_type_base<int S::*, long S::*>, empty_type));

static_assert(__is_same(common_type_base<int (S::*)(), int (S::*)()>, type_identity<int (S::*)()>));
static_assert(__is_same(common_type_base<int (S::*)(), int (T::*)()>, type_identity<int (T::*)()>));
static_assert(__is_same(common_type_base<int (S::*)(), long (S::*)()>, empty_type));

