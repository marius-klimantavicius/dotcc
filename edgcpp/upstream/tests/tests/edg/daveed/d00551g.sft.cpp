//remark:GNU __builtin_offsetof
//type:fp
//name:
//options:
//options_all:--g++ -tused --gnu_version=40000
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

template<class T> int g(int (*)[sizeof(T().T::X::x)]) {
	return 3;
}

template<class T> int f(int (*)[__builtin_offsetof(T, x)]) {
	return 3;
}


template<class U> struct E {
	int j[sizeof(f<typename U::N>(0))];
};


struct X {
	int x0;
	int x;
};

struct S: X {
};



//int i = g<S>((int(*)[sizeof(int)])0);
int X::*pm = &X::x;
int j = f<S>(0);

