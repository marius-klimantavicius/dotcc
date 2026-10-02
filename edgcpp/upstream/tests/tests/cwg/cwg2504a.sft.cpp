//type:fn
//options_all:--c++23 -tused
struct B1{
     B1(int, ...) {}
};
struct B2{
     B2(double) {}
};
struct D2 : B2 {
    using B2::B2;
    B1 b;
};
D2 f(1.0);

//cwg: 2504
//title: Inheriting constructors from virtual base classes
//meeting: Kona 11/23
//edg_status: EDGcpfe/26787
