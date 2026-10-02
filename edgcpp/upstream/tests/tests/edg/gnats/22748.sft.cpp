//options_all:--gn 40801 --c++11
namespace std {
    template <class> struct initializer_list;

    struct unordered_set {
        explicit unordered_set();
        unordered_set(initializer_list<int>);
    };
}

struct Foo {
    int id;
    std::unordered_set set;
};

void f() {
    Foo f({ 0, { } });
};
