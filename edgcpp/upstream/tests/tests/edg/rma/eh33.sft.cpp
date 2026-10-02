//options_all:-r -x -tused
//options: --strict;rp

void f() throw (int);

int main()
{
        extern void f() throw (int);
        extern void f() throw (int);
}


