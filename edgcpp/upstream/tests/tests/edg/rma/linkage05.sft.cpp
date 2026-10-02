//options_all:-r -x -tused
//options: --strict;cp

class A {}; class B{}; class C {}; class D {};
class E { A B::* p; };
class F { C D::* p; };
static E x;
extern F y;

