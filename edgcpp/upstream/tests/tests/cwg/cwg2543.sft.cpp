//type:fn
//options_all:--c++20 -tused -A
float f;
constinit int * pi = (int*) &f;    // reinterpret_cast, not constant-initialized

//cwg: 2543
//title: constinit and optimized dynamic initialization
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/26069
//fixed_in: 6.5
