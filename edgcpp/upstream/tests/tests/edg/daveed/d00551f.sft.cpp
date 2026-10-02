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

struct S { int i, j, k; };
int a[__builtin_offsetof(S, k)];

template<class T> struct X {
	int a[__builtin_offsetof(T, k)];
};

X<S> x;
