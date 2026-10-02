//remark:GNU C zero length arrays
//type:rp
//name:
//options:--gcc;rp:--g++;rp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#ifdef __cplusplus
extern "C"
#endif
int printf(char const*, ...);
int a[] = {};

struct S {
	int z;
	int a[0];
} s = { 1, {} };

int b[0][0];

int main() {
	printf("sizeof(a[] = {}) = %d\n", sizeof(a));
	return 0;
}
