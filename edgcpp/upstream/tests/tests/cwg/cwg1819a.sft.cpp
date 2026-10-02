//type:fp
//options_all:--c++17 -tused -A
template<class T> struct A {
struct C {
template<class T2> struct B { };
template<class T2> struct B<T2**> { }; // partial specialization #1
};
};
// partial specialization of A<T>::C::B<T2>
template<class T> template<class T2>
struct A<T>::C::B<T2*> { }; // #2
A<short>::C::B<int*> absip; // uses partial specialization #2

//cwg: 1819
//title: Acceptable scopes for definition of partial specialization
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
