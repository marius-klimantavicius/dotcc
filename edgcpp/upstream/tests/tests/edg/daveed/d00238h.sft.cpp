//remark:Microsoft __interface support
//type:fn
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:  
//output_files:
//ulimit:
//linker_options:
//execution_args:

__interface X {
	int a;
};


__interface Y {
	static void f();
	static int i;
};

int Y::i = 46;

__interface Z {
	static int const i = 23;
};

int main() {
	try {
	} catch (X) {}
}

