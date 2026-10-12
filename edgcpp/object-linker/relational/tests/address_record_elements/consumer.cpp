#include "program.h"
Holder* selected_holder();
volatile int* selected_values();
int selected_index();
volatile int* field_address() { return &selected_holder()->field; }
volatile int* indexed_address() { return &selected_values()[selected_index()]; }
