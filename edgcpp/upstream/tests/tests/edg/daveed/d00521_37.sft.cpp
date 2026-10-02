//remark:Microsoft DLL attributes
//type:fp
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
//script:

void f() {
	extern __declspec(dllexport) int x;
	extern __declspec(dllexport) int g();
}
__declspec(dllexport) int x;
__declspec(dllexport) int g();

