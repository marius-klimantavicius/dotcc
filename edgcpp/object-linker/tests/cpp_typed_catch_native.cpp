#include "cpp_typed_catch.cpp"
extern "C" int printf(const char *, ...);
extern "C" int puts(const char *);
extern "C" void _Exit(int);
namespace std { typedef void (*terminate_handler)(); terminate_handler set_terminate(terminate_handler) noexcept; }
static void terminated() { _Exit(77); }
static int trace[128], count, failure;
extern "C" void record_event(int value) { trace[count++]=value; }
extern "C" int should_fail(int n) { return n==failure; }
int main(int argc, char **) {
    if(argc>1) { failure=21; std::set_terminate(terminated); typed_case(4); return 1; }
    for(int mode=0;mode!=14;++mode) {
        count=0;
        try { record_event(300+typed_case(mode)); }
        catch(const TypedDerived &value) { record_event(500+value.value); }
        catch(long value) { record_event(600+(int)value); }
        printf("%d",mode);
        for(int i=0;i<count;++i) printf(" %d",trace[i]);
        puts("");
    }
}
