//options_all:-r -x -tused
//options: --strict;cp

template <int *p> class A8 {};
int i8;
void f13(int &A8)
{
	&A8 < &i8;
}


template <int i> class A9 {};
void f9(void *p)
{
	struct A9 {} **A9;
	(void*)*A9 < p;
}


