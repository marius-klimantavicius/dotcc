//options_all:-r -x -tused
//options: --strict;cn

// #016 712p11a: inline can only be used in function declarations
inline class X { public: int i; };

int main()
    {
    X x;
    return x.i = 0;
    }

