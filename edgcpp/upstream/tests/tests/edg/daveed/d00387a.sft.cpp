//remark:External name conflicts with global variables
//type:fn
//name:
//options:;fn:-A;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int x, y, z;

namespace N {
	extern "C" int x;
	extern "C" {
		int y;
	}
	extern "C" void z();

	extern "C" int a;
	extern "C" {
		int b;
	}
	extern "C" void c();
}

int a, b, c;

