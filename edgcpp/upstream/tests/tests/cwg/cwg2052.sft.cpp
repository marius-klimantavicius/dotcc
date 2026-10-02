//type:fp
//options_all:--c++17 -tused -A
struct A { operator int() { return 1; } };
template<class T> T operator<<(T, int) { return T(); }
void b(A a) { 1 << a; } 
int mymain() { return 0; }

//cwg: 2052
//title: Template argument deduction vs overloaded operators
//meeting: Kona 10/15
//edg_status: Passes
