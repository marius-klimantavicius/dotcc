//remark:Type of selective overrider's *this parameter
//type:fp
//name:
//options:
//options_all:--microsoft_version=1310
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:
__interface A
{
    void    fct();
};

class    B : public A
{
    void    load()
    {
    }

    void    A::fct()
    {
        load();
    }
};
