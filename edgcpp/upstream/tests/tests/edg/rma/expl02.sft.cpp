//options_all:-r -x -tused
//options: --strict;cn

struct A {
	explicit A(int);
};

struct B {
	template <class T> explicit B(T);
};

void g(A);
void h(B);

void f()
{
	g(1);  // gets expected error
	h(1);  // incorrectly accepted
}


