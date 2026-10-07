#include "variable_asm_type.h"
extern "C" int puts(const char*);
void construct_object();
void destroy_object();
void* original_address();
int third_translation_unit();
void* alternate_address();
int alternate_read();
namespace demo {
extern Object object __asm__("object_storage");
extern int initialized __asm__("initialized_storage");
extern int mangled_view __asm__("_ZN8original5valueE");
extern int renamed_view __asm__("renamed_storage_symbol");
extern __thread int thread_view __asm__("thread_storage");
}
int main() {
    if (third_translation_unit()!=11 || constructions || destructions) return 1;
    void* volatile a=&demo::object;
    void* volatile b=alternate_address();
    if(a!=b || a!=original_address() || ((unsigned long)a&31))return 2;
    if(demo::initialized!=43 || demo::thread_view!=17)return 3;
    demo::initialized=61;
    demo::thread_view=79;
    if(demo::mangled_view!=89 || demo::renamed_view!=97)return 7;
    demo::mangled_view=103;
    demo::renamed_view=107;
    construct_object();
    if(constructions!=1 || destructions || demo::object.read()!=57 || alternate_read()!=57)return 4;
    demo::object.value=73;
    if(alternate_read()!=73)return 5;
    destroy_object();
    if(constructions!=1 || destructions!=1 || demo::initialized!=61 || demo::thread_view!=79)return 6;
    if(demo::mangled_view!=103 || demo::renamed_view!=107)return 8;
    puts("asm symbol views: typed storage, initialization and lifetime passed");
    return 0;
}
