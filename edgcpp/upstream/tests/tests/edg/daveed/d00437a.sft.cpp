//remark:Empty GNU C structs
//type:rp
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

struct S {} s;
int x[1+sizeof(s)];

extern int printf(char const*, ...);

struct A { int:0; };

int main() {
	printf("%d %d\n", sizeof(s), sizeof(struct A));
	return 0;
}
