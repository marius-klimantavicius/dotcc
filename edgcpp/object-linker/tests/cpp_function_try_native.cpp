#include "cpp_function_try.cpp"
extern "C" int printf(const char *, ...);
extern "C" int puts(const char *);
static int trace[128],count,selected,mode;
extern "C" void record_event(int value) { trace[count++]=value; }
extern "C" int should_fail(int value) { return value==selected; }
extern "C" int current_mode() { return mode; }
int main() {
    const int failures[]={0,1,2,3,7,10,21,22};
    const int delegation_failures[]={0,3,8};
    for(mode=0;mode!=6;++mode) for(int index=0;index!=(mode<4?8:mode==4?3:1);++index) {
        int failure=mode<4?failures[index]:mode==4?delegation_failures[index]:0;
        // Avoid deliberately throwing a second destructor during unwinding.
        if(mode==3 && (failure==21 || failure==22)) continue;
        selected=failure;count=0;
        record_event(400+function_try_case(mode));
        printf("%d:%d",mode,failure);
        for(int i=0;i<count;++i)printf(" %d",trace[i]);
        puts("");
    }
}
