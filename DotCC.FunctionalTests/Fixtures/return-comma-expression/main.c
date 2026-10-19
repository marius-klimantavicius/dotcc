#include <stdio.h>
struct entry { int first, second; };
static void tick(int *depth) { ++*depth; }
static int leave(int *depth,int value) { return --*depth,value; }
static int *select_pointer(int *depth,int *value) { return tick(depth),value; }
static struct entry select_entry(int *depth,struct entry value) { return tick(depth),value; }
static unsigned char narrow(int *depth,int value) { return tick(depth),value; }
int main(void) {
    int depth=3, value=7;
    int result=leave(&depth,42);
    int *pointer=select_pointer(&depth,&value);
    struct entry input={9,10};
    struct entry output=select_entry(&depth,input);
    int small=narrow(&depth,258);
    printf("%d %d %d %d %d %d\n",result,depth,*pointer,output.first,output.second,small);
    return 0;
}
