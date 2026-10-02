//options_all:-r -x -tused
//options: --strict;cn

void f()
{
        union {
                unsigned asint[2];
                double asdouble;
        };
        asdouble = val;
        return asint[0] ^ asint[1];
}

