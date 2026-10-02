//remark:Designators into annonymous union
//type:cp
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:


struct foo {
  union {
    int x;
    char y[4];
  };

  int z;
};

struct foo f = {
  {x:10},
  z:15
};
