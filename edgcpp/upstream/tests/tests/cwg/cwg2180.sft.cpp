//type:fn
//options_all:--c++17 -tused -A
struct A {
   A() = default;
private:
   A& operator=(const A&) = default;
};

struct B: virtual A {
   B() = default;
   virtual void f() = 0;
   B& operator=(const B&) = default;
};

struct C : B {
   C() = default;
   virtual void f() { }
   C& operator=(const C&) = default;
};

void f(C& c0, C& c1) {
   c0 = c1;
}

//cwg: 2180
//title: Virtual bases in destructors and defaulted assignment operators
//meeting: Jacksonville 2/16
//edg_status: EDGcpfe/22212
