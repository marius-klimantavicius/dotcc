#include "program.h"
int assigned_result(Packed* value, int input) {
    return (value->low = input);
}
int main() {
    Packed value = {};
    return assigned_result(&value, 15) == 7 ? 0 : 1;
}
