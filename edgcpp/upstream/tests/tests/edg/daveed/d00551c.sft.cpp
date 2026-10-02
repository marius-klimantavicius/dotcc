//remark:GNU __builtin_offsetof
//type:fn
//name:
//options:
//options_all:--g++ -tused --no_defer --gnu_version=40000
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

template<class T> struct B {
	template<class U> struct N {
		int f;
		int n;
	};
};

template<class T> struct D: virtual B<T>::template N<T> {
	virtual ~D() {}
};

extern "C" int printf(char const*, ...);

template<class T> void f() {
	printf("From f<T>: %d\n",
	       __builtin_offsetof(D<T>, B<T>::template N<T>::n));
}


int main() {
	f<int>();
	printf("From main: %d\n",
	       __builtin_offsetof(D<double>, B<double>::N<double>::n));
	D<double> d;
	printf("Manual: %d\n", (char*)&d.n - (char*)&d);
}
