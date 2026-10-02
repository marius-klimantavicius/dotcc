//remark:GNU __builtin_offsetof
//type:fp
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

template<typename T> struct S {};

template<typename T> void f() {
	typename S<T>::X obj;
	int a[sizeof(obj.X::x)];
	__builtin_offsetof(T, X::x);
	__builtin_offsetof(S<T>, x);
	__builtin_offsetof(typename S<T>::X, x);
	obj.X::x;
}
