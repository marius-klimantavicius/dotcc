//options_all:-r -x -tused
//options: --strict;cn:;cn

struct A {
	struct B {};
	enum E {e1, e2};
	typedef int	T;
};

main()
{
	B	b;
	E	e;
	T	t;

	e = A::e1;	// This works
	e = A::e2;	// This works

	e = e1;		// This doesn't
	e = e2;		// This doesn't
}

