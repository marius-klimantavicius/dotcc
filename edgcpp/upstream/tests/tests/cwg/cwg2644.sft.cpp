//type:fn
//options_all:--c++20 -tused -A
auto z = [a = 42](int a) { return 1; };   // error: parameter and conceptual local variable have the same name

//cwg: 2644
//title: Incorrect comment in example
//meeting: Kona 11/22
//edg_status: EDGcpfe/25821
