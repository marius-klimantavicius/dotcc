//type:fp
//options_all:--c++17 -A -tused


constexpr struct A {
int x;
struct B {
     int i;
     int j;
} b;
} a = { 1, { 2, 3 } };
 int main()
 {
    static_assert( a.x == 1);
    static_assert( a.b.i == 2);
    static_assert( a.b.j == 3);
}

//cwg: 2359
//title: Unintended copy initialization with designated initializers
//meeting: Rapperswil 6/18
//edg_status: Passes
