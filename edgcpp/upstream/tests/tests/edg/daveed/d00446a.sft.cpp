//remark:Floating-point in-class initializers for static data members
//type:rp
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern "C" int printf(char const*, ...);

struct S {
	static double const F = 3.2;
};
double const S::F;
int main() {
	printf("%g\n", S::F);
	return 0;
}
