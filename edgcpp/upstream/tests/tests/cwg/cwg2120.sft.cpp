//type:fp
//options_all:--c++17 -tused -A
struct B {int a[3]; };
B b;
static_assert(__is_standard_layout(B));

//cwg: 2120
//title: Array as first non-static data member in standard-layout class
//meeting: Kona 10/15
//edg_status: Passes
