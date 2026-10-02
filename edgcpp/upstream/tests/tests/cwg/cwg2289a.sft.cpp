//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
struct A { int x; } a; 
  auto [A] = a; // ok? 

//cwg: 2289
//title: Uniqueness of structured binding names
//meeting: Kona 02/19
//edg_status: EDGcpfe/21015
