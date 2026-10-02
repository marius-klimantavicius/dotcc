//type:cp
//options:--c++11:--c++17

template <class T> T f(T) { return {}; }
template <class T> struct A {
    template <class U> A(U, U u = f(U())){}
};
template <class T> struct B : A<T> {
    using A<T>::A;
};
A<int> a(0), a2(0, 1);
B<int> b(2), b2(2, 3);
