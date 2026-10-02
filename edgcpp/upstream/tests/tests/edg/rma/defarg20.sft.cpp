//options_all:-r -x -tused
//options: --strict;cn

template < void (*pf)(int=0) > void g() { }

struct A {
        void f(double);
};
struct B : public A { };
template < void ( B :: * p ) ( double = 0) > void f ( int ) { } 
void g()
{
        f<&B::f>(37);
}

