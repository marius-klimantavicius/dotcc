#include <stdlib.h>
#include <string.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
static char borrowed[]="BORROW=one";
int environment_seed(void){char *p=getenv("EDG_ENV_SYNTHETIC_SEED");return !p?0:strcmp(p,"first")==0?1:strcmp(p,"second")==0?2:3;}
static int startup_seed;
__attribute__((constructor(101))) static void observe_environment_startup(void){startup_seed=environment_seed();}
int environment_startup_seed(void){return startup_seed;}
int environment_set_seed(void){return setenv("EDG_ENV_SYNTHETIC_SEED","translated",1);}
void *environment_table(void){return environ;}
int environment_borrow(int edit){if(edit){borrowed[7]='t';borrowed[8]='w';borrowed[9]='o';}return putenv(borrowed);}
char *environment_borrow_value(void){return getenv("BORROW");}
int environment_cases(void)
{
    char owned[]="BORROWED=abc";char *manual[]={ (char*)"DUP=one",(char*)"DUP=two",(char*)"OTHER=value",0 };
    if(clearenv()!=0 || environ!=0)return 1;
    errno=71;if(getenv("MISSING") || getenv("") || getenv("A=") || errno!=71)return 2;
    if(setenv("A","one",0)!=0 || strcmp(getenv("A"),"one") || errno!=71)return 3;
    char *first=getenv("A");if(first!=getenv("A") || setenv("A",(const char*)1,0)!=0 || first!=getenv("A"))return 4;
    if(setenv("A","two",1)!=0 || strcmp(getenv("A"),"two"))return 5;
    if(setenv("EMPTY","",1)!=0 || !getenv("EMPTY") || *getenv("EMPTY"))return 6;
    if(putenv(owned)!=0 || getenv("BORROWED")!=owned+9)return 7;
    owned[9]='z';if(strcmp(getenv("BORROWED"),"zbc"))return 8;
    if(unsetenv("BORROWED")!=0 || getenv("BORROWED") || owned[9]!='z')return 9;
    if(putenv((char*)"A")!=0 || getenv("A"))return 10;
    errno=0;if(setenv("","x",1)!=-1 || errno!=EINVAL)return 11;
    errno=0;if(setenv("A=B","x",1)!=-1 || errno!=EINVAL)return 12;
    errno=0;if(unsetenv("A=B")!=-1 || errno!=EINVAL)return 13;
    errno=0;if(putenv((char*)"")!=-1 || errno!=EINVAL)return 14;
    /* C callers may replace environ with their own pointer vector. */
    clearenv();environ=manual;
    if(strcmp(getenv("DUP"),"one") || setenv("DUP","updated",1)!=0 || strcmp(getenv("DUP"),"updated") || strcmp(manual[1],"DUP=two"))return 15;
    if(unsetenv("DUP")!=0 || getenv("DUP") || environ!=manual || strcmp(environ[0],"OTHER=value") || environ[1])return 16;
    if(setenv("NEW","heap",1)!=0 || environ==manual || strcmp(getenv("OTHER"),"value") || strcmp(getenv("NEW"),"heap"))return 17;
    if(clearenv()!=0 || environ!=0)return 18;
    errno=71;if(unsetenv("ABSENT")!=0 || errno!=71)return 19;
    if(setenv("RAW","\xff\x80",1)!=0 || (unsigned char)getenv("RAW")[0]!=255 || (unsigned char)getenv("RAW")[1]!=128)return 20;
    clearenv();return 0;
}
unsigned long environment_digest(void)
{
    unsigned long h=1469598103934665603UL;unsigned int state=0x19eeU;int present[8]={0},value[8]={0};char key[]="K0",text[]="v0";
    clearenv();
    for(int step=0;step<20000;++step){state=state*1664525U+1013904223U;unsigned int i=state>>29,op=(state>>24)%3;key[1]=(char)('0'+i);text[1]=(char)('a'+(state%26));
        if(op==2){if(unsetenv(key))return 0;present[i]=0;}else{if(setenv(key,text,(int)op))return 0;if(op || !present[i])value[i]=text[1];present[i]=1;}
        for(int j=0;j<8;++j){key[1]=(char)('0'+j);char *p=getenv(key);if((p!=0)!=present[j] || (p && (p[0]!='v' || p[1]!=value[j] || p[2])))return 0;h=(h^(p?(unsigned char)p[1]:255))*1099511628211UL;}}
    clearenv();return h;
}
#ifdef __cplusplus
}
#endif
