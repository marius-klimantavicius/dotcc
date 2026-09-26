#include "pinta_tests.h"
extern void *heap;
#define PINTA_CASE(name) void name(void);
#include "native-cases.inc"
#undef PINTA_CASE

/* Compare actual fixture bytes through the same callbacks used by the VM.
 * All these names occur naturally on Windows, Unix or mixed-path hosts. */
static int check_fixture_resolution(void) {
    const wchar *names[]={
        L"unicode-string-v2.pint",
        L"..\\Marius.Pinta.Test.Files\\unicode-string-v2.pint",
        L"../Marius.Pinta.Test.Files/unicode-string-v2.pint",
        L"C:\\src\\pinta\\Marius.Pinta.Test.Files\\unicode-string-v2.pint",
        L"C:/src/pinta/Marius.Pinta.Test.Files/unicode-string-v2.pint",
        L"../Marius.Pinta.Test.Files\\unicode-string-v2.pint"};
    for(unsigned i=0;i<sizeof(names)/sizeof(names[0]);i++) {
        void *reference=pinta_platform_file_open(NULL,(void*)names[0],(uint32_t)wcslen(names[0]));
        void *actual=pinta_platform_file_open(NULL,(void*)names[i],(uint32_t)wcslen(names[i]));
        int ok=reference!=NULL && actual!=NULL;
        if(ok) {
            uint32_t size=pinta_platform_file_size(NULL,reference),total=0;
            unsigned char left[64],right[64];
            ok=size>0 && size==pinta_platform_file_size(NULL,actual);
            while(ok) {
                uint32_t count=pinta_platform_file_read(NULL,reference,left,sizeof(left));
                uint32_t other=pinta_platform_file_read(NULL,actual,right,sizeof(right));
                if(count!=other || memcmp(left,right,count)) {ok=0;break;}
                total+=count;
                if(!count)break;
            }
            ok=ok && total==size;
        }
        if(reference)pinta_platform_file_close(NULL,reference);
        if(actual)pinta_platform_file_close(NULL,actual);
        if(!ok)return 1;
    }
    puts("PASS fixture resolution: 6 path forms, exact bytes");
    return 0;
}
int main(int argc, char **argv) {
    struct {const char *name;void (*run)(void);} groups[]={
        {"gc",pinta_tests_gc},{"array",pinta_tests_array},{"memory",pinta_tests_memory},
        {"code",pinta_tests_code},{"code_v2",pinta_tests_code_v2},{"buffer",pinta_tests_buffer},
        {"format",pinta_tests_format},{"integer",pinta_tests_integer},{"decimal",pinta_tests_decimal},
        {"weak",pinta_tests_weak},{"encoding",pinta_tests_encoding},{"property",pinta_tests_property},
        {"object",pinta_tests_object},{"function",pinta_tests_function},{"pattern",pinta_tests_pattern},{"json",pinta_tests_json}};
    setvbuf(stdout,NULL,_IONBF,0);
    if(argc==2 && strcmp(argv[1],"--check-fixtures")==0)return check_fixture_resolution();
    if(argc==3 && strcmp(argv[1],"--case")==0) {
        struct {const char *name;void (*run)(void);} cases[]={
#define PINTA_CASE(name) {#name,name},
#include "native-cases.inc"
#undef PINTA_CASE
        };
        int found=0;sput_start_testing();sput_enter_suite(argv[2]);
        for(unsigned i=0;i<sizeof(cases)/sizeof(cases[0]);i++)if(strcmp(argv[2],cases[i].name)==0){sput_run_test(cases[i].run);found=1;}
        if(!found)return 2;sput_finish_testing();
    }
    else if(argc==1)pinta_tests();
    else {int found=0;sput_start_testing();for(unsigned i=0;i<sizeof(groups)/sizeof(groups[0]);i++)if(strcmp(argv[1],groups[i].name)==0){groups[i].run();found=1;}if(!found)return 2;sput_finish_testing();}
    int result=sput_get_return_value();free(heap);return result;
}
