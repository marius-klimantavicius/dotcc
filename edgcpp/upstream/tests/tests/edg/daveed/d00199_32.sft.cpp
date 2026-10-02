//remark:C++ VLAs
//type:fp
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

int n = 10;

typedef int (*T)[5];

T f()
{
	typedef int T[n];

	T x;

	return &x;
}
