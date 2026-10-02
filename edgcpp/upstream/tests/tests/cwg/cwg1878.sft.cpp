//type:fn
//options_all:--c++17 -tused -A
 struct S {
    template<class T> operator auto() { return 42; }
  };

//cwg: 1878
//title: operator auto template
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/21348
