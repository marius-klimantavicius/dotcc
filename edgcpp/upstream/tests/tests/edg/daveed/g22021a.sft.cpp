//remark:Substitution and nontype template arguments
//options:--c++17;fp

template <class>
using void_t = void;

template <typename T, int Extent = 9001>
struct span {
    static constexpr int extent = Extent;

    template <int ex = extent>
    constexpr span() {}

    template <typename Container, typename = void_t<decltype(Container().size())>>
    constexpr span(Container &cont) {}
};

template <class Container>
span(Container &) -> span<typename Container::value_type>;

template <class T>
struct vector {
    using value_type = T;
    int size() const;
};

void foo(vector<char> &v) {
    span{ v };
}
