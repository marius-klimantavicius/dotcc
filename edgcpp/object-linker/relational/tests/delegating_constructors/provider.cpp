#include "program.h"
int trace = 0;
int arguments = 0;
int selected_argument(int value) {
    ++arguments;
    if (value == -2) throw 34;
    return value;
}
Value::Value(int value, int) : self(this), value(value) {
    trace = trace * 10 + 1;
    if (value < 0) throw 31;
}
Value::Value(int value) : Value(selected_argument(value), 0) {
    trace = trace * 10 + 2;
    if (value == 99) throw 32;
}
Value::Value(int value, bool fail) : Value(value) {
    trace = trace * 10 + 3;
    if (fail) throw 33;
}
