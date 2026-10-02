//remark:Empty GNU attributes
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

  __attribute__((,)) void a(void);
  __attribute__((,,,,)) void b(void);
  __attribute__((pure,,,,)) void c(void);
  __attribute__((,,,,pure)) void d(void);
