//remark:SFINAEing ptr/ref to const member function type
//options:--c++11;fp:--c++11 -A;fp

template <typename T, typename U> struct is_same {
    static constexpr bool value = false;
};
template <typename T> struct is_same<T, T> {
    static constexpr bool value = true;
};
template <typename T> struct AlwaysVoid {
    typedef void type;
};
template <typename T, typename = void> struct AddRef {
    typedef T Lvalue;
    typedef T Rvalue;
};
template <typename T> struct AddRef<T, typename AlwaysVoid<T&>::type> {
    typedef T& Lvalue;
    typedef T&& Rvalue;
};
template <typename T> struct add_lvalue_reference {
    typedef typename AddRef<T>::Lvalue type;
};
template <typename T> struct add_rvalue_reference {
    typedef typename AddRef<T>::Rvalue type;
};
template <typename T, typename = void> struct AddPtr {
    typedef T type;
};
template <typename T> struct AddPtr<T, typename AlwaysVoid<T *>::type> {
    typedef T * type;
};
template <typename T> struct add_pointer {
    typedef typename AddPtr<T>::type type;
};
static_assert(is_same<add_lvalue_reference<int>::type, int& >::value, "BOOM");
static_assert(is_same<add_rvalue_reference<int>::type, int&&>::value, "BOOM");
static_assert(is_same<         add_pointer<int>::type, int *>::value, "BOOM");
static_assert(is_same<add_lvalue_reference<void>::type, void  >::value, "BOOM");
static_assert(is_same<add_rvalue_reference<void>::type, void  >::value, "BOOM");
static_assert(is_same<         add_pointer<void>::type, void *>::value, "BOOM");
using Weird = int (int) const;
static_assert(is_same<add_lvalue_reference<Weird>::type, Weird>::value, "BOOM");
static_assert(is_same<add_rvalue_reference<Weird>::type, Weird>::value, "BOOM");
static_assert(is_same<         add_pointer<Weird>::type, Weird>::value, "BOOM");

