#include "program.h"
int main() {
    int trace = 0;
    {
        Derived object(&trace);
        Base *view = &object;
        if (selected_value(view) != 2) return 1;
        if (trace != 12) return 2;
        if (selected_additional(view) != 7) return 4;
        int count = 0;
        if (selected_once(view, &count)->plus(++count) != 4 || count != 2) return 7;
    }
    if (trace != 1221) return 3;
    Both both;
    Right *second = &both;
    if (selected_right(second) != 50) return 5;
    Left *first = &both;
    if (first->read_left() != 40) return 6;
    return 0;
}
