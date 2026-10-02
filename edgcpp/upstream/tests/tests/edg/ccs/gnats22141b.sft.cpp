//type:cp
//options::--microsoft_version 1920
//options_all:--c++17 -w

template <class T, unsigned int N>
struct array {};

template <class T>
struct remove_cv { using type = T; };
template <class T>
struct remove_cv<const T> { using type = T; };

template <typename T, unsigned int Extent>
struct span {
    using value_type = typename remove_cv<T>::type;

    template <unsigned int N>
    constexpr span(const array<value_type, N> &arr) {}
};

template <class T, unsigned int N>
span(const array<T, N> &) -> span<const T, N>;

void foo(const array<int, 3> values) {
    const span value_span{ values };
}
