//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
// was a Plum Hall test in 17xvs _14823Y61, but core issue 349 was dropped with 2384
//
struct A {
	template <class T> operator T***() {
		int*** p = 0;
		return p;
	}
};

struct B {
	template <class T> operator T***() {
		const int*** p = 0;
		return p;
	}
};

int main()
{
	A a;
	const int * const * const * p1 = a;
	B b;
	const int * const * const * p2 = b;
}

//cwg: 2384
//title: Conversion function templates and qualification conversions
//meeting: Kona 02/19
//edg_status: Passes
