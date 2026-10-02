//type:fp
//options_all:--c++17 -tused -A
   struct A { A(int); };
   struct B { B(A); };
   B b{{0}};

//cwg: 2076
//title: List-initialization of arguments for constructor parameters
//meeting: Oulu 6/16
//edg_status: Passes
