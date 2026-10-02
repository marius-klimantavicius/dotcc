//type:fn
//options_all:--c++17 -A

alignas(double) void f(); 

//cwg: 2205
//title: Restrictions on use of alignas
//meeting: Kona 2/17
//edg_status: Passes
