/* C/C++ jumps may enter ordinary blocks, loops and switch-controlled loops.
   Structured C# is used only when its scopes retain these exact transfers. */
static int duff_copy(unsigned char *to, const unsigned char *from, int count)
{
    int rounds=(count+3)/4;
    switch(count%4) {
    case 0: do { *to++=*from++;
    case 3:      *to++=*from++;
    case 2:      *to++=*from++;
    case 1:      *to++=*from++;
            } while(--rounds>0);
    }
    return rounds;
}
static int enter_loop(int input)
{
    int result=0;
    goto in_loop;
    while(input>0) {
        result+=10;
in_loop:
        result+=input;
        if(--input==1) break;
    }
    return result;
}
static int enter_branch(int input)
{
    int result=0;
    goto inside;
    if(input==0) {
inside:
        result=7;
    } else result=99;
    return result;
}
static int sections(int mode)
{
    int result=0;
    switch(mode) {
    case 0:
    case 1: result+=2;
    case 2: result+=3; break;
    default: result=9;
    }
    return result;
}
static int nested_control(int count)
{
    int result=0;
    for(int outer=0;outer<count;++outer) {
        for(int inner=0;inner<5;++inner) {
            if(inner==1)continue;
            switch(inner) {
            case 2: continue;
            case 3: result+=outer;break;
            default: ++result;
            }
            if(inner==4)break;
        }
    }
    return result;
}
int main(void)
{
    unsigned char source[11]={1,2,3,4,5,6,7,8,9,10,11},copy[13]={0};
    if(duff_copy(copy+1,source,11)!=0 || copy[0] || copy[12])return 1;
    for(int index=0;index<11;++index)if(copy[index+1]!=source[index])return 2;
    if(enter_loop(4)!=29 || enter_branch(5)!=7)return 3;
    if(sections(0)!=5 || sections(1)!=5 || sections(2)!=3 || sections(8)!=9)return 4;
    if(nested_control(4)!=14)return 5;
    return 0;
}
