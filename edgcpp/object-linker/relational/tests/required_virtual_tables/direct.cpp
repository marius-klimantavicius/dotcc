#include "program.h"

int main() {
    External value(9);
    return value.External::eval(4) == 13 ? 0 : 1;
}
