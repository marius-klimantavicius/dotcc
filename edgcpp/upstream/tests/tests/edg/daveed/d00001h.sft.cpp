/*
//remark:Ordinary designators
//type:rp
//name:
//options:
//options_all:--c --designators
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

#if defined __cplusplus
extern "C"
#endif
int printf(char const*, ...);

int a[10] = { [5] = 100, 101, 102, 103, [3] = 105, 106, 107, 108 };

int main() {
	int k;
	for (k = 0; k<10; ++k) { printf("%4d", a[k]); }
	printf("\n");
	return 0;
}

