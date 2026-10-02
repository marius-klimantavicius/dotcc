//type:fn
//options:--c++14:--c++26
//options_all:-A

struct S {
  operator auto() { return 0; } // error
};

//cwg: 1670
//title: auto as conversion-type-id
//meeting: Kona 11/25
//edg_status: EDGcpfe/28532
