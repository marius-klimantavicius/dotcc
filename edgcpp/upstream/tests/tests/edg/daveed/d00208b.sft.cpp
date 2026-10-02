//remark:UPC THREADS folding
//type:fn
//name:
//options:
//options_all:--upc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

typedef struct {
	int A;
	int B;
} Biggy;

shared Biggy a[2*MYTHREAD];
shared Biggy b[MYTHREAD*4];
Biggy c[(MYTHREAD*4)/(3*MYTHREAD)];
shared Biggy b[THREADS+THREADS];
