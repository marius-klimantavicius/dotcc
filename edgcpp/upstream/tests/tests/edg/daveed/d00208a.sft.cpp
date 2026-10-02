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

shared Biggy a[2*THREADS];
shared Biggy b[THREADS*4];
Biggy c[(THREADS*4)/(3*THREADS)];
shared Biggy d[THREADS+THREADS];
