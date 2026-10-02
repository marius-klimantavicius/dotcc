//options_all:--microsoft --c++14
struct A {
    A() noexcept;
    ~A() throw(int);
};
static_assert(noexcept(A{}), "A{} is NOT noexcept");
static_assert(!noexcept(A{}), "A{} is noexcept");

struct B {
    ~B() throw(int);
};
static_assert(noexcept(B{}), "B{} is NOT noexcept");
static_assert(!noexcept(B{}), "B{} is noexcept");
