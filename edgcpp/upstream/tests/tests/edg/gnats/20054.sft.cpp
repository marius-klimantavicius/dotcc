//options_all:--microsoft --c++17
struct base {};
struct derived : public base {};

void f(derived &d) {
    static_assert(noexcept(dynamic_cast<base&>(d)), "");
}
