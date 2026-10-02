//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --cfront_3.0;rp:;rp

// Different behavior and diagnostics in cfront 2.1 mode
extern "C" int printf(char *,...);
int flag = 0;
 
struct B {};

struct D : public B {
    operator B() { flag++; return *((B*)this); }
};

main()
{
    D d;
    (B)d;
    printf("flag=%d\n", flag);
    return flag;
}

