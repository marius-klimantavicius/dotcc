#include "program.h"
Holder* volatile selected_pointer = nullptr;
volatile int* unsupported_address() { return &selected_pointer->field; }
