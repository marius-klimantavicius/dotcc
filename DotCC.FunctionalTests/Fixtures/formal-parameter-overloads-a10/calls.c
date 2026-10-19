#include "contract.h"
#include <time.h>
#include <string.h>
#include <stdio.h>
#include <math.h>

static int calls;
static int trace;
static unsigned short next_capacity(void) {
    calls++;
    trace=trace*10+calls;
    return 64;
}
int side_calls(void) { return calls; }
int side_trace(void) { return trace; }

int run_c(void) {
    time_t epoch=0;
    struct tm *calendar=gmtime(&epoch);
    char storage[66];
    char *output=storage+1;
    char empty_format[]={0};
    size_t (*callback)(char *,size_t,const char *,const struct tm *)=strftime;
    size_t (*external)(size_t)=choose;
    const size_t qualified=64;
    size_t wide=((size_t)1<<31)+65;
    size_t ordinary,tight,empty;
    float fraction=1.25f;
    _Bool truth=1;
    signed char negative=-2;
    unsigned short narrow=65535;
    enum Shade { THREE=3 };
    if(!calendar) return 1;
    memset(storage,0x5a,sizeof(storage));
    ordinary=strftime(output,64,"%Y-%m-%d %H:%M:%S",calendar);
    if(ordinary!=19 || strcmp(output,"1970-01-01 00:00:00") || output[19]!=0) return 2;
    if(storage[0]!=0x5a || storage[65]!=0x5a) return 3;
    if(strftime(output,wide,"%Y-%m-%d %H:%M:%S",calendar)!=19) return 4;
    if(strftime(output,qualified,"%Y",calendar)!=4) return 5;
    tight=strftime(output,4,"%Y",calendar);
    empty=strftime(output,1,empty_format,calendar);
    if(tight!=0 || empty!=0 || output[0]!=0 || storage[0]!=0x5a || storage[65]!=0x5a) return 6;
    if(callback(output,64,"%Y",calendar)!=4 || callback!=strftime) return 7;
    if(strftime(output,truth,empty_format,calendar)!=0) return 8;
    if(fabs(sin(fraction)-0.9489846193555862)>1e-14 || fabs(sin(truth)-0.8414709848078965)>1e-14) return 9;
    if(snprintf(output,64,"%d/%d/%.2f/%d",negative,narrow,fraction,THREE)!=15) return 10;
    if(strcmp(output,"-2/65535/1.25/3")) return 11;
    if(choose(64)!=1064 || external(4)!=1004 || external!=choose) return 12;
    if(strftime(output,next_capacity(),"%Y",calendar)!=4) return 13;
    if(strftime(output,next_capacity(),"%m",calendar)!=2) return 14;
    if(0) { strftime(output,next_capacity(),"%Y",calendar); }
    if(calls!=2 || trace!=12) return 15;
    return 0;
}
