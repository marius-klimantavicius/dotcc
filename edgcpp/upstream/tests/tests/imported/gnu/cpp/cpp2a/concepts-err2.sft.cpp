//type: fn
//options: --c++20
// PR c++/95735
// { dg-do compile { target concepts } }

template <auto F>
    requires requires { F(); }
bool v{};

void f() {
    int x;
    static_assert(v<[&] { x++; }>); // { dg-error "not a constant expression" }
}
