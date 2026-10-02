//remark:friend virtual
//type:fp
//name:
//options:--microsoft;fp:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct Foo {
    virtual void func();
};

struct Bar {
    friend virtual void Foo::func();
};

