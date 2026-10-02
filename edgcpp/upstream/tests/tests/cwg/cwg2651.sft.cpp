//type:fn
//options_all:--c++20 -tused
template<class T,bool B>
using get=T(*)() noexcept(B);

struct A {
 template<class T>
 operator get<T,false>() const;
};

auto *p=A().operator get<int,true>();

//cwg: 2651
//title: Conversion function templates and "noexcept"
//meeting: Kona 11/22
//edg_status: EDGcpfe/25822
