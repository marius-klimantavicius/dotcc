//options_all:-r -x -tused
//options: --strict;cn

class A { int a(), b(), c(); };
class B { friend class A; friend int A::a(), A::a(); friend class A; };

