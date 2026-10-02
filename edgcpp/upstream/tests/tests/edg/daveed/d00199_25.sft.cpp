//remark:C++ VLAs
//type:fp
//name:
//options:--gcc;fp:--g++;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void f(int n) {
	typedef int (*pva)[n];
	typedef int (va)[n];
	struct S {
		double x[sizeof(pva)];
		double y[sizeof(va)];
	};
}

