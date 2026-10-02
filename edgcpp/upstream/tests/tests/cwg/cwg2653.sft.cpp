//type:fn
//options_all:--c++23 -tused
struct S { void f(this const S& = S{}); };

//cwg: 2653
//title: Can an explicit object parameter have a default argument?
//meeting: Kona 11/22
//edg_status: EDGcpfe/25846
