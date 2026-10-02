//options_all:-r -x -tused
//options: --strict;cn

struct A {
	struct B {};
};

struct A::B {
	int i;
};


