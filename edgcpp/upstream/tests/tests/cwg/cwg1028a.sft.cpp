//type:fn
//options_all:--c++20 -tused -A
struct B {
int f(int);
};
struct D : B {
int f(const char*);
};
// Here D::f(const char*) hides B::f(int) rather than overloading it.
void h(D* pd) {
pd->f(1); // error:
// D::f(const char*) hides B::f(int)
}

//cwg: 1028
//title: Dependent names in non-defining declarations
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23865
