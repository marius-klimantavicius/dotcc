#include "variable_asm_type.h"
namespace alternate { extern demo::Object view __asm__("object_storage"); }
int third_translation_unit() { return 11; }
void* alternate_address() { return &alternate::view; }
int alternate_read() { return alternate::view.read(); }
