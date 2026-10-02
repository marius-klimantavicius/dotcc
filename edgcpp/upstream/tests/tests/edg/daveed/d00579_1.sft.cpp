//remark:GNU C/C++ complex type support
//type:rp
//name:
//options:--gcc:--g++;fn
//options_all:--gnu_version=40000
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

_Complex double z = 2.0 + 3.0*__I__;

#ifdef __cplusplus
extern "C"
#endif
int printf(char const*, ...);

int main() {
	printf("%f\n", (float)z);
}

