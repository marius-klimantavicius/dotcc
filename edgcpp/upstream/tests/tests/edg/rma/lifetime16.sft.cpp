//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

int ctor_calls, dtor_calls;
class A {
public:
	A() { ctor_calls += 1; }
	~A() { dtor_calls += 1; }
};
main()
{
	int x=1;
	ctor_calls = dtor_calls = 0;
	switch (x) {
		case 20:
			if (x) {
		default:	A a;
			}
	}
}

