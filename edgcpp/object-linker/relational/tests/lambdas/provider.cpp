#include "program.h"

int body_effects = 0;
int capture_effects = 0;

int selected_value(int value) { ++capture_effects; return value; }

int captureless_call(int input) {
    return [](int value) { ++body_effects; return value + 3; }(input);
}

int& captureless_reference(State& state) {
    return [](State& selected) -> int& {
        ++body_effects;
        return selected.value;
    }(state);
}

int captured_values(int input) {
    int original = input;
    auto copy = [original](int extra) { ++body_effects; return original + extra; };
    original = 1000;
    return copy(5);
}

int captured_reference(int& input) {
    auto reference = [&input](int delta) { ++body_effects; input += delta; return input; };
    return reference(7);
}

int capture_once(int input) {
    auto captured = [value = selected_value(input)](int extra) {
        ++body_effects;
        return value + extra;
    };
    return captured(2);
}

int converted_call(int input) {
    int (*selected)(int) = [](int value) { ++body_effects; return value + 11; };
    return selected(input);
}
