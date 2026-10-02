//options_all:-r -x -tused
//options: --strict;cn:;cp

// EDGma00120 -- n1140904
namespace N {
    int f() { return 1; }
    namespace M {
        void g()
        {
            class X {
                friend int f(); // error : no prior definition of f()
                                //         in this scope
            };
        }
    }
}

