//options_all:-r -x -tused --microsoft
//options:;rp:;cn

template <class T> struct B {
	int f() {
		T t1;
		T t2;
		int i = 0;
		if (t1 == t2) i = 1;
		return i;
	}
};

template <class T> struct A {
	friend void f(B<T> t) {
		x;
		t.f();
	}
};

struct C {};

int main()
{
	A<C> ai;
	return 0;
#if TEST_NUMBER==2
	B<C> bc;
	bc.f();
	bc.f();
#endif
}

