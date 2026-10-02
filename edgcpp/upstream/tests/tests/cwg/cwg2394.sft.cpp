//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
   struct B { const A a; };
   B b;   

//cwg: 2394
//title: Const-default-constructible for members
//meeting: Kona 02/19
//edg_status: EDGcpfe/21021
