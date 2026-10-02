//options_all:-r -x -tused
//options: --strict;cn:;rp

static union {
        short x;
};

main()
{
        union {
                short x;
        };
        x = 37;
        ::x = 47;
        if (x != 37 || ::x != 47)
                return 1;
        return 0;
}


