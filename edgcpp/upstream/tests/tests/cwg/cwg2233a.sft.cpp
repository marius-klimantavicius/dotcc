//type:fn
//options_all:-tused --c++17 -A
 template<typename ...T> int f(int n = 0, T ...t);
  int x = f<int>();   // error: no argument for second function parameter

//cwg: 2233
//title: Function parameter packs following default arguments
//meeting: Rapperswil 6/18
//edg_status: EDGcpfe/21901
