//remark:Qualified declaration should not bind to declaration in unnamed namespace
//type:fn
//name:
//options:;fn:--g++ --gnu_version=30300;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

namespace {
	void f();
	void f(int);
}

int ::f;
