//remark:GNU __builtin_offsetof
//type:rp
//name:
//options:--gnu_version=40000:--gnu_version=30400;fn
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

struct A {
	A();
	int i;
	int k;
};

extern "C" int printf(char const*, ...);

int main() {
	printf("%d %d\n", __builtin_offsetof(A, i),
	                  __builtin_offsetof(A, k));
	return 0;
}
