//remark:Aggregate initializer syntax
//type:fp
//name:
//options:--gcc:--c;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct foo {
        int a;
};

struct foo bar = {{{1}}};
struct foo baz = {{{1},},};

