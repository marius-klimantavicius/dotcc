#include "pinta_tests.h"
extern void *heap;
#define PINTA_CASE(name) void name(void);
#include "native-cases.inc"
#undef PINTA_CASE
int main(int argc, char **argv) {
    struct {const char *name;void (*run)(void);} groups[]={
        {"gc",pinta_tests_gc},{"array",pinta_tests_array},{"memory",pinta_tests_memory},
        {"code",pinta_tests_code},{"code_v2",pinta_tests_code_v2},{"buffer",pinta_tests_buffer},
        {"format",pinta_tests_format},{"integer",pinta_tests_integer},{"decimal",pinta_tests_decimal},
        {"weak",pinta_tests_weak},{"encoding",pinta_tests_encoding},{"property",pinta_tests_property},
        {"object",pinta_tests_object},{"function",pinta_tests_function},{"pattern",pinta_tests_pattern},{"json",pinta_tests_json}};
    setvbuf(stdout,NULL,_IONBF,0);
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
