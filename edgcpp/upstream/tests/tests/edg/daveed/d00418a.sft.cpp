//remark:Dynamic initializers in flexible array initializers
//type:fn
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

struct F {
        int n;
        int a[];
};

int printf(char const*, ...);

void f(int n) {
        struct F f = { 2, printf("3\n") };
}

