//remark:Using of nontags in presence of tag declarations
//type:fp
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

namespace A {
	typedef struct x {} x;
}

namespace B {
	using A::x;
	struct x v;
}


namespace C {
	struct x {};
}

namespace D {
	using C::x;
}

namespace C {
	typedef struct x x;
}

namespace D {
	using C::x;
}


namespace E {
	struct x {};
}

namespace F {
	typedef E::x x;
}

namespace E {
	using F::x;
}

