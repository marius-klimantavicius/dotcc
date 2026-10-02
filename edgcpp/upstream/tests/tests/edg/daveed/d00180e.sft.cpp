//remark:UPC Extensions
//type:fn
//name:
//options:;fn:--c99;fn:--gcc;fn
//options_all:--upc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int printf(char const*, ...);

int main() {
	printf("%x\n", &THREADS);
	return 0;
}
