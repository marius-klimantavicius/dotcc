//remark:GNU: deprecated attribute
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

struct S {
	int x;
} __attribute__((deprecated));

extern struct S a __attribute__((deprecated));

void f(void*) __attribute__((deprecated));

int main() {
	f(&a);
}
