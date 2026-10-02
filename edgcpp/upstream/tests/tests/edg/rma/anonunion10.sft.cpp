//options_all:-r -x -tused
//options: --strict;cn

// check use of types with anonymous unions

int E;

static union {
	enum E {ee};
	typedef char*& T;
	struct A {};
	static int xxx;
};

char *pc;
T t = pc;
char* A;

enum E e;
struct A a;

