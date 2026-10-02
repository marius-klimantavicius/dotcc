//type:cp
//options::--microsoft_version 1920
//options_all:--c++17

template <typename>
struct A {
    A(...) {}
};

template <class T>
A(T)->A<T>;

const A a{1};
