//remark:The __thread specifier
//type:fp
//name:
//options:--sun:--gnu_version=30400 --gcc:--gnu_version=30400 --g++:--gnu_version=30200 --g++;fn:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

__thread static int x;

__thread extern int y = 3;

void f() {
	x = 3;
}
