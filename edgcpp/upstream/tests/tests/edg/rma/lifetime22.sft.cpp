//options_all:-r -x -tused
//options: --strict;cn:;rp

// Correct output is:
//    foo==foo
//    bar==bar
//    foo==foo

extern "C" int printf(const char *, ...);
extern "C" int strlen(const char *);
extern "C" char *strcpy(char *, const char *);

struct X {
	char *p;
	X(char *);
	~X();
};

X::X(char *_p)
{
	p = new char[strlen(_p)+1];
	strcpy(p, _p);
}

X::~X()
{
	delete [] p;
}

void foo()
{
	static const X &rX = "foo";
	printf("foo==%s\n", rX.p);
}

void bar()
{
	const X &rX = "bar";
	printf("bar==%s\n", rX.p);
}

main()
{
	foo();
	bar();
	foo();
	return 0;
}


