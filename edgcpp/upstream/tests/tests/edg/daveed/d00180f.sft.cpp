//remark:UPC Extensions
//type:fn
//name:
//options:;fn:--c99;fn:--gcc;fn
//options_all:--upc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() {
   int N = THREADS;
   int Q = 2*THREADS*2;
	float M = 1.0*MYTHREAD;
	strict shared [4] int a[THREADS];
	upc_barrier 3;
	upc_notify 4;
	upc_wait 5;
	upc_fence;
	upc_forall (a[0] = 1; ;; a[2]) {
	}

}
