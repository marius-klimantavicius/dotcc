//remark:GNU C local labels
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

int label = 3;

void f() {
	__label__ x;
	({
		__label__ a;
		int i;
		({
			__label__ a;
			int i;
			a: i = 2;
		});
		a: i = 2;
	});
	({
		__label__ a;
		int i;
		a: i = 2;
	});
	x: return;

}

