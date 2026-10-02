//remark:GNU C zero length arrays
//type:fp
//name:
//options:
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
   int nr_of_values;
   union U {
      int   value;
      char* string;
   } values[0];
};

struct S myS = { 3, { { string: "A" }, { string: "B" }, { string: "C" }
} };
