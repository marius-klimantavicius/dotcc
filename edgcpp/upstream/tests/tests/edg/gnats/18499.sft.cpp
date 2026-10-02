//type:fp
//options_all:--c++11 --g++
template<class T>
struct S {
    void f() noexcept(noexcept(v)) {}
    int v;
};
