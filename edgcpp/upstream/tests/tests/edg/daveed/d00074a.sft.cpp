//remark:Misformed linkage specifier
//type:fn
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct A {
        A();
        struct B {};
        static B x;
        B extern "C++"; 
};

