//remark:C++ VLAs
//type:fp
//name:
//options:
//options_all:--g++ -x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

struct S {
	S(): i(3) {}
	~S() {}
	int i;
};

int f(int n) {
	S a[n];
	a[2].i = 5;
	return a[n/2].i;
}

int main() {
	f(5);
}
