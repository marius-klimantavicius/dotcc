//remark:CTAD and missing initializers
//options:--c++17;fn

template <class ...T> struct A {
int x[29];
};

constexpr A a;
