//type:cp
//options:--c++11:--c++17

template <class T> T f(T){return T();}
template <class T> struct A {
    static const int y = 1;
    template <class U=int> A(int, U u = f(U())+y) : x(u) {}
    int x;
};
template <class T> struct B : A<T> {
    void *y;
    using A<T>::A;
};
A<int> a(0);
B<int> b(1);
