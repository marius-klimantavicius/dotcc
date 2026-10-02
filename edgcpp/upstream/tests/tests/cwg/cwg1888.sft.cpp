//type:fn
//options_all:-tused --c++17 -A
//
struct A { explicit A(){} };
struct B { A a; };

B b = {};

//cwg: 1888
//title: Implicitly-declared default constructors and explicit
//meeting: Lenexa 5/15
//edg_status: Passes
