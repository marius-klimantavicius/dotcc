//remark:GNU C attributes
//type:fp
//name:
//options:--gcc;fp:--g++;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct {
  int b:1 __attribute__((packed));
} c;

