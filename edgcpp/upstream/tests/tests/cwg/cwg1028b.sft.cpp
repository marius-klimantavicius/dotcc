//type:fp
//options_all:--c++20 -tused -A
struct B {
int f(int);
};
struct D : B {
int f(const char*);
};
// Here D::f(const char*) hides B::f(int) rather than overloading it.
void h(D* pd) {
// D::f(const char*) hides B::f(int)
pd->B::f(1); // OK
pd->f("Ben"); // OK, calls D::f
}
