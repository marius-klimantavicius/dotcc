//remark:GNU C/C++ complex type support
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
//script:

#include <typeinfo>

__complex double z = 0.0i;

#ifdef __cplusplus
extern "C"
#endif
int printf(char const*, ...);

int main() {
	__real z = 1;
	__imag z = 2.0;
	printf("%g %g\n", __real z, __imag z);
	printf("%s\n", typeid(__real (_Complex float)z).name());
	double r = __real(z+z) * __imag(z/z);
	return 0;
}
