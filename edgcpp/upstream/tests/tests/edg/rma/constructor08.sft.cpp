//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cn

struct A {
	struct B {
		A::B(){}
	};
};

//test 2
struct X {
	struct B {
		union C {
			X::C(){}
		};
	};
};


