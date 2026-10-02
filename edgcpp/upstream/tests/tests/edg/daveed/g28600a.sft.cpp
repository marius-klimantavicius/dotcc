//remark:Disambiguation for auto cast
//options:--c++17 --gnu=140200;fp:--c++23;fp

template<typename T> void f() noexcept(noexcept((auto(T{}))));
