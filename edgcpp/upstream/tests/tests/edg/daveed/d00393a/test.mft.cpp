//remark:-D/-U in GNU mode
//type:rp
//name:
//options:--g++:;rp
//options_all:--export -UM -DM
//cases:
//source_files:t2.c
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:


export template<class T> void f();

int main() {
	f<int>();
}
