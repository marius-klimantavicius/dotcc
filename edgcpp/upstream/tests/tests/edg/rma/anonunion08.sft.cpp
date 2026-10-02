//options_all:-r -x -tused
//options: --strict;cn

static union {
	enum E {ee};
	typedef char*& T;
	struct A {};
	struct B {
		struct C {
		};
	};
};

//test 1
typedef int E;

//test 2
class T {};

//test 3
enum A {aa};

//test 4
struct B {
	struct C {
	};
};

