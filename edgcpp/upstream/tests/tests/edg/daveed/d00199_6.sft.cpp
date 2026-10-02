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

int f(int n) {
	int a[n];
	a[2] = 5;
	return a[n/2];
}

int main() {
	f(5);
}
