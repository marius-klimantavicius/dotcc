//remark:C++11 constexpr constraint
//options:--gnu=59999 --c++11 --no_defer;fp:--c++11;fn

template<typename T>
struct S {
    constexpr bool f() {}
};
S<int> s;
