//remark:Extraneous namespace qualification
//type:fp
//name:
//options:--g++;fp:--microsoft;fn:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

namespace N
{
    struct A
    {
        // Shouldn't work according to stroustrup
        friend void foo();
    private:
        int x;
    };

    A a;

    void foo()
    {
        a.x = 0;
    }
}

namespace N1
{
    struct A
    {
        // Works with g++, not with edg --g++
        friend void N1::foo();
    private:
        int x;
    };

    A a;

    void foo()
    {
        a.x = 0;
    }
}
