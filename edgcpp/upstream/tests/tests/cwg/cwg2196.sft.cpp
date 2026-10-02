//type:rp
//options_all:--c++17 -tused -A

struct V { int n; };
struct A : virtual V {};
struct B : A {
  B() : V{123}, A() {}
} b;

int main()
{
    return(b.n);  //required to be 0 not 123
}

//cwg: 2196
//title: Zero-initialization with virtual base classes
//meeting: Kona 2/17
//edg_status: EDGcpfe/21992
