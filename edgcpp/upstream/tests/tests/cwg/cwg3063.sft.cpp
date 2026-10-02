//type:fn
//options:--c++11:--c++26
//options_all:-A -tused

struct B { ~B(); };
struct A { const B &b; };

A foo() { return {{}}; }   // error

void bar();

int main()
{
  A a = foo();
  bar();
}

//cwg: 3063
//title: Lifetime extension of temporaries past function return
//meeting: Kona 11/25
//edg_status: EDGcpfe/28545
