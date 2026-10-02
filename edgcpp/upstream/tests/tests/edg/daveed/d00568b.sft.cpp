//remark:GNU asm symbolic operands
//type:fp
//name:
//options:--gcc:--g++:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

  float f(float a) {
    register float r;
    asm("fsinx %[Rads], %[Res]": [Res]"=f"(r): [Rads]"f"(a));
    return r;
  }
