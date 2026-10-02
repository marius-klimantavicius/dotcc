//type:fp
//options_all:--c++17 -tused -A
//
template<typename ...Ts> struct X { X(int); };
template<typename T> using Y = int;
template<typename ...Ts> void f() {
  X<Y<Ts>...> x;
}

//cwg: 2024
//title: Dependent types and unexpanded parameter packs
//meeting: Kona 10/15
//edg_status: EDGcpfe/22007
