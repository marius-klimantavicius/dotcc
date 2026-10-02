//remark:Intrinsic alias templates
//options:--c++14 --set_flag=alias_templ_intrinsics;fp:--c++14 --clear_flag=alias_templ_intrinsics;fp

namespace std {
  template<typename T> struct remove_const { using type = T; };
  template<typename T> struct remove_const<const T> { using type = T; };
  template<typename T> using remove_const_t = typename remove_const<T>::type;

  template<typename T> struct remove_volatile { using type = T; };
  template<typename T> struct remove_volatile<volatile T> { using type = T; };
  template<typename T> using remove_volatile_t = typename remove_volatile<T>::type;

  template<typename T> struct remove_reference { using type = T; };
  template<typename T> struct remove_reference<T&> { using type = T; };
  template<typename T> struct remove_reference<T&&> { using type = T; };
  template<typename T> using remove_reference_t = typename remove_reference<T>::type;

  template<typename T> struct remove_cv {
    using type = remove_const_t<remove_volatile_t<T>>;
  };
  template<typename T> using remove_cv_t = typename remove_cv<T>::type;
  template<typename T> struct remove_cvref {
    using type = remove_cv_t<remove_reference_t<T>>;
  };
  template<typename T> using remove_cvref_t = typename remove_cvref<T>::type;
}

using namespace std;

template<typename, typename> constexpr bool are_same_types = false;
template<typename T> constexpr bool are_same_types<T, T> = true;

using RCVI = int const volatile&;
using CVI = int const volatile;
static_assert(are_same_types<remove_const_t<CVI>, int volatile>, "");
static_assert(are_same_types<remove_volatile_t<CVI>, int const>, "");
static_assert(are_same_types<remove_cv_t<CVI>, int>, "");
static_assert(are_same_types<remove_cvref_t<CVI>, int>, "");
static_assert(are_same_types<remove_cvref_t<RCVI>, int>, "");

