//remark:dynamic_cast<void*> diagnostic
//options:--c++17;fp

template<class T, T v>
struct integral_constant {
    static constexpr T value = v;
};
 
using true_type = integral_constant<bool, true>;
using false_type = integral_constant<bool, false>;
 
namespace detail {
 
template <class T>
true_type detect_is_polymorphic(
    decltype(dynamic_cast<const volatile void*>(static_cast<T*>(nullptr)))
);
template <class T>
false_type detect_is_polymorphic(...);
 
} // namespace detail
 
template <class T>
struct is_polymorphic : decltype(detail::detect_is_polymorphic<T>(nullptr)) {};
 
struct S { int i; };
 
static_assert(!is_polymorphic<S>::value);
