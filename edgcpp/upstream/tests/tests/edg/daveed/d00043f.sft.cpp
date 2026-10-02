//remark:Using declarations with mix of tag and nontags
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

namespace N {
	struct S {};
	namespace NN {
		void S();
	}
}

void f() {
	using N::NN::S;
	S();
	struct S s;
}

