//options_all:-r -x -tused
//options: --strict;cn

struct A { A(); A(A&); A(const A&); A(volatile A&); A(const volatile A&); };
struct B { B(const B&); };
struct C : public A, public B { C(volatile C&) {} };
struct D : public A, public B { D(const volatile D&) {} };
struct E : public A, public B { E(const E&) {} };

