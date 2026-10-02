//remark:__declspec and specializations
//type:fn
//name:
//options:;fn:-DPOS;fp
//options_all:--microsoft --diag_error 1215
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template<class T> void f();
template<> __declspec(deprecated) void f<void>();

int main() {
#ifndef POS
	f<void>();
#endif
	f<int>();
	return 0;
}

