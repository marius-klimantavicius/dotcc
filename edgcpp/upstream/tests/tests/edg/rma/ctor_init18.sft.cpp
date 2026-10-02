//options_all:-r -x -tused
//options: --strict;cp

struct B { B(int); };

void f() {
    int B;
    struct D : public B {
	D() : B(3) {}
    };
}

