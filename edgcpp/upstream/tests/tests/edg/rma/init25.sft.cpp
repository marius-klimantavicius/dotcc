//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

extern "C" int printf(char*, ...);
int counter = 0;

class M {
public:
	int m;
	M() { m = ++counter; }
	M(int i) : m(-i) { }
	M(const M& t) { m = t.m; }
	bool operator != (int i) { return (m != i); }
};

const M m0(0), m1(1), m2(2);

struct S {
	M sm0;
	M sm1;
	M sm2;
	M sm3;
	M sm4;
};

main() {
	counter = 0;

	S s = { m0, m1, m2 };
        printf("%d\n", counter);
}

