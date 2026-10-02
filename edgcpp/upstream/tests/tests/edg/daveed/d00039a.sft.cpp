//remark:Near match on member redeclaration
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


class V {
    public:
            static int init(unsigned n, unsigned long m);
    };
    class C {
            friend void V::init(unsigned, unsigned long);
    };
