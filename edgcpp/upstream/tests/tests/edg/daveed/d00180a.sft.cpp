//remark:UPC Extensions
//type:fp
//name:
//options:;fp:--c99;fp:--gcc;fp
//options_all:--upc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f1() {}

#pragma upc strict

#pragma STDC FP_CONTRACT OFF
void f2(int i) {
	if (i) {
#pragma upc relaxed
	}
}

#pragma upc strict
#pragma upc relaxed
void f3() {
}
