//remark:Microsoft dllimport/dllexport compatibility
//type:fn
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() {
	if (int __declspec(dllexport) i = 2) {}
}

