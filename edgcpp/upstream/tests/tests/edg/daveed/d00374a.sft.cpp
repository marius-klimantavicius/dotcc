//remark:Compound assignment and fixed-point
//type:fp
//name:
//options:
//options_all:--embedded_c
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() {
unsigned long _Accum a;
short _Fract b;
a += b;
b += a;
}
