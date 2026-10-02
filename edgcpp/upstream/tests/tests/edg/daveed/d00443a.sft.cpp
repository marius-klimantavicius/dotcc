//remark:The __thread specifier
//type:fn
//name:
//options:--sun:--gnu_version=30400 --gcc:--gnu_version=30400 --g++:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

__thread struct S {
	__thread float field;
	__thread static int sm;
};


typedef __thread int I;


__thread int ti1;

__thread void f() {
	__thread int a;
}
