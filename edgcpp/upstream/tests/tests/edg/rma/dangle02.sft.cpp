//options_all:-r -x -tused
//options: --strict;cn:;cn

template <class T> struct A {
	static int x;
};

template <class T> struct B {
	typedef struct {
		static int x;
	} A;	// Spurious error here
};

int A<int>::x = 37;

int B<int>::A::x = 47;


