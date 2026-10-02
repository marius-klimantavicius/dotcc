//type:fp
//options_all:--c++20 -tused
consteval int const_div(int a, int b) { return a / b; }
int func(int x = const_div(10, 0));

//cwg: 2631
//title: Immediate function evaluations in default arguments
//meeting: Kona 11/22
//edg_status: EDGcpfe/25837
