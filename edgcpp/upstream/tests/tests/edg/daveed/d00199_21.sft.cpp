//remark:C++ VLAs
//type:fn
//name:
//options:
//options_all:--g++ -x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

// ???
// VLA types used with operator T()

void f() {
	int n = 10;

	typedef int T[n];

	struct A {
void e(int n) { double x[n]; }
void d() { T volatile x; }
#if 1
void m(T const) {}
operator T const T ; 
#endif
	};
}
