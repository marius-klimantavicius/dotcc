//options_all:-r -x -tused
//options: --strict;cn

// Access control on enumerations
class A {
private:
    friend class B;
    enum { privateEnum };
};

class B {
private:
    static int i;
};

class C {
private:
    static int i;
};

int B::i = A::privateEnum;  // access permitted (B is a friend of A)
int C::i = A::privateEnum;  // no access

