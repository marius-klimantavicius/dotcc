//options_all:-r -x -tused
//options: --strict;cn:;cn

#ifdef PROTO
int foo(int a, enum E { x,y,z } *b) { return a; }
#else
int foo(a,b) int a; enum E *b; { return a; }
#endif

enum E { x,y,z };
 
main()
{
    enum E b;
    foo(1, &b);
}

