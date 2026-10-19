#include <stdio.h>
static int mode(const char *cursor, int external) {
    int chosen=0, plus=0, effects=0;
    if(external) goto bad_mode;
    while(*cursor) {
        switch((++effects,*cursor++)) {
        case 'r':
            if(chosen) { bad_mode: effects+=100; goto error; }
            chosen=1; break;
        case '+': if(plus) goto bad_mode; plus=1; break;
        case 'b': break;
        default: effects+=1000; goto error;
        }
    }
    if(!chosen) goto bad_mode;
    return effects+chosen+plus;
error: return -effects;
}
static int loop(int external) {
    int i=0, effects=0, total=0;
    if(external) goto entered;
    for(i=0;i<3;++i) {
        switch(++effects) {
        case 1: {
            int values[2]={++total,5};
            if(!external) total+=values[0]+values[1];
            entered: total+=i;
            if(i==1) continue;
            break;
        }
        default: total+=20; break;
        }
        total+=10;
    }
    return total+100*effects;
}
static int nested(int external) {
    int effects=0;
    if(external) goto target;
    switch(++effects) {
    case 1:
        switch(++effects) {
        case 2: target: effects+=10; break;
        default: effects+=100; break;
        }
        effects+=20; break;
    default: effects+=1000; break;
    }
    return effects;
}
int main(void) {
    printf("%d %d %d %d %d\n",mode("r+",0),mode("rr",0),mode("",0),mode("b",0),mode("r",1));
    printf("%d %d %d %d\n",loop(0),loop(1),nested(0),nested(1));
    return 0;
}
