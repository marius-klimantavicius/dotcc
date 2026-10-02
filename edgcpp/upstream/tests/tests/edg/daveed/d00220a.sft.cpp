//remark:GNU C transparent unions
//type:fn
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

typedef union U {
	char *str;
	double d;
} U __attribute__((transparent_union));

void f(int, U);

int main() {
	f(2, "hello");
	f(2, 7.0);
	return 0;
}

