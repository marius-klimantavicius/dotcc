//remark:Match exception specifications on variables
//type:fn
//name:
//options:
//options_all:-x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct A {
	static void (A::*pmf)() throw (int);
};

void (A::*A::pmf)() throw();


