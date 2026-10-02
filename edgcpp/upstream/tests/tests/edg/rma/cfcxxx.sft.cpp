//options_all:-r -x -tused
//options: --strict;cp

class V1 { };
class V2 : virtual public V1 { };
class A : virtual public V2 { };
class B : virtual public V1 { };
class C : public A, public B { };
C c;

