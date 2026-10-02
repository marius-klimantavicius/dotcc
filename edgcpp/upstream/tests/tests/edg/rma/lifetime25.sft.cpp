//options_all:-r -x -tused
//options: --cfront_3.0;rp

// This program triggers an internal error if -b/--cfront_2.1 is applied.
// All is well with --cfront_3.0.

struct A {
    A() {}
    ~A() {}
};

void func_switch_for1(int x)
{
    switch (x)
        case 1:
            for (A a2;1;)
                break;

}

main()
{
    func_switch_for1(1);
}

