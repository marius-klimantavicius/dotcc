//options_all:-r -x -tused
//options: --strict;cn:;rp

template<class P> struct X {
        void foo(int pfn(int)) { }
};
extern int ext_func(int);
main()
{
X<int> x;
x.foo(ext_func);
return 0;
}

