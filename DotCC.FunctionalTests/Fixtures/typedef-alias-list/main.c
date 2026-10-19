#include <stdio.h>
#include "types.h"
int main(void) {
    Word input={3,5}; Alias output=transform(input);
    NumberAlias number={0x01020304UL};
    KindAlias kind=KIND;
    Scalar scalar=9; SecondPointer pointer=&scalar;
    Rows rows={{1,2,3},{4,5,6}};
    Values values={1,2,3,4};
    { typedef long Scalar, Local; Local value=13; if(sizeof(Scalar)!=8 || value!=13) return 1; }
    printf("%lu %lu %lu %d %d %d %d %lu\n",output.low,output.high,number.wide,
        kind,*pointer,rows[1][2],values[3],(unsigned long)sizeof(scalar));
    return 0;
}
