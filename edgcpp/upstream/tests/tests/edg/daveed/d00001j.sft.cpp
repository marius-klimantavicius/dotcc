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
int printf( char const*, ...);

int a[][4] = {
	[200000] = { [2] = 121, 169 },
	[100000] = { 1, 4, 9 },
	[200000][1] = 36, 49, 64
};

int main() {
	int k, l;
	for (k = 0; k<sizeof(a)/sizeof(int[4]); ++k) {
		for (l = 0; l<4; ++l) {
			if (a[k][l]) { printf("a[%d][%d] = %d\n", k, l, a[k][l]); }
		}
	}
	printf("sizeof(a) = %d\n", sizeof(a));
	return 0;
};

