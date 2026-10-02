//type:fn
//options_all:--c++17 -A -tused 
//
class C { };
void f(int(C)) { } // void f(int(*fp)(C c)) { }
// not: void f(int C) { }
int g(C);
void foo() {
f(1); // error: cannot convert 1 to function pointer
f(g); // OK
}

//cwg: 2259
//title: Unclear context describing ambiguity
//meeting: Kona 2/17
//edg_status: Passes
