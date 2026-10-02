//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

extern "C" int printf (char *, ...);
struct A {
    A() { printf ("A() called\n"); }
    A(const A&) 
    { 
        static int i = 0;
        printf ("A(const A&) called\n");
        if (++i == 2) {
            printf ("Throwing exception\n");
            throw 0;
        }
    }
    ~A() {printf ("~A() called\n"); }
};


struct B {
    A a[5];  // Implicit copy constructor generated for B.
};

main()
{
    printf("Default init of c\n");
    B c;
    try {
       printf("Default init of b\n");
       B b = c;  
    }
    catch (...) {
        printf ("Caught in main\n");
    }
    printf("About to return\n");
    return 0;
}

