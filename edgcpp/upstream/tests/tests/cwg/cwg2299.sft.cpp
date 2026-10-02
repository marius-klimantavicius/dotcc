//type:fp
//options_all:--c++17 -tused -A
//this test fails because the standard says "it is unspecified where e is a core constant expression
#include <stdarg.h>
struct A_ {
	int a;
};
int a_(A_ a, ...) {
	va_list b;
	va_start(b, a);
	int d = va_arg(b, int);
	va_end(b);
	return d;
}
int b_ = a_(A_(), 2);

int main()
{
	// _CXXWP - Implements p0968r0 - 2019
	// _CXXWP - Implements core 2299 - 2019
	int a[b_] = { 0 };
	
}

//cwg: 2299
//title: constexpr vararg functions
//meeting: Jacksonville 2/18
//edg_status: Passes
