//remark:Overloading ::main
//type:fn
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int main();

namespace N {
	void main();
}

using N::main;

int main() {
}

