//remark:Flexible array members with destructible types
//type:fp
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S { ~S(); };
struct T { S s[]; };
struct U { T t; };

S s;
U u;

