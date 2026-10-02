//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

extern "C" int printf(...);
int b0;

struct S {
         int i[5];
         S(){ printf("S(%ld)\n",(char*)this-(char*)b0);}
         S(int j){ printf("S(%ld) j=%d\n",(char*)this-(char*)b0, j);}
        ~S(){ printf("~S(%ld)\n",(char*)this-(char*)b0);}
         S(S& x){printf("S(%ld,%ld)\n",(char*)this-(char*)b0,&x);};
 } x;

struct array {
S mem[5];
} s1, s2=s1, s3[5] = {1, 2, 3, 4, 5};

int main(){}


