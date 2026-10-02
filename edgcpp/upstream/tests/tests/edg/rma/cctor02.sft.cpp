//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a,b,c; A(const A&); A(A&); A(); };
A a1;               // A()
A a2 = a1;          // A(A&)
const A a3;         // A()
const A a4 = a3;    // A(const A&)
A a5 = a3;          // A(const A&)
class B { public: int a,b,c; B(B&); B(); };
B b1;               // B()
B b2 = b1;          // B(B&)
const B b3;         // B()
const B b4 = b3;    // error
B b5 = b3;          // error
class C { public: int a,b,c; C(const C&); C(); };
C c1;               // C()
C c2 = c1;          // C(const C&)
const C c3;         // C()
const C c4 = c3;    // C(const C&)
C c5 = c3;          // C(const C&)

