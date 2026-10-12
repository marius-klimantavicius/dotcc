#include "program.h"
int (*selected_address)(int) = address_helper;
int provider(int value, int *counter) {
    Guard guard(counter);
    return called_helper(value);
}
