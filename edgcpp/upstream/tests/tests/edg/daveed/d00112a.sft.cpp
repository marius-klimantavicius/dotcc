//remark:GNU C void and function types
//type:fp
//name:
//options:
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void *p, *q;

void f() {
	p = q+4;
	f+4;
	f-f;
}

int printf(char const*, ...);

int main() {
	printf("sizeof(void) = %d, sizeof(func) = %d\n",
	       sizeof(void), sizeof(void()));
}

