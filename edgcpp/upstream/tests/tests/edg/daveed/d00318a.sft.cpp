//remark:Missing initializers
//type:fp
//name:
//options:--microsoft;fp:;fn:-A;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

const struct {
    unsigned long jjj;
} A;
const struct {
    unsigned long kkk;
} B[3];

// these don't
const enum {
    HELLO
} X;
const enum {
    BYE
} Y[2];


enum { k } const x[2];
