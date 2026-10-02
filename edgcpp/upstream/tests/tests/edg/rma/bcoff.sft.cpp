//options_all:-r -x -tused
//options: --strict;cp

class A { int a,b,c; };
class B { int a,b,c; };
class C : public B, public A { int a,b,c; };
class D { int a,b,c; };
class E { int a,b,c; };
class F : public D, public E { int a,b,c; };
class G : public F, public C { int a,b,c; };

