//remark:Microsoft mode bit fields in union allocation
//type:rp
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

extern "C" int printf(char const*, ...);

union U {
	char c: 3;
	char d: 4;
};

int main() {
	printf("sizeof(U)==%d\n", sizeof(U));
	return 0;
}

