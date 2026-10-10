// Discarded branches are syntactically valid but would be semantically invalid
// for int if instantiated. Only the resolved branch belongs to execution.
template<class T> int selection_true(T value) {
    if constexpr (sizeof(T) == sizeof(int)) return value + 3;
    else return value.missing_member();
}
template<class T> int selection_false(T value) {
    if constexpr (sizeof(T) > 100) return value.missing_member();
    else return value + 5;
}
template<class T> int selection_no_else(T value) {
    if constexpr (sizeof(T) > 100) return value.missing_member();
    return value + 7;
}
template<class T> int selection_nested(T value) {
    if constexpr (sizeof(T) == sizeof(int)) {
        if constexpr (alignof(T) >= alignof(int)) return value + 11;
        else return value.missing_member();
    } else return value.other_missing_member();
}
struct SelectionGuard {
    int *trace;
    explicit SelectionGuard(int *value) : trace(value) { *trace = *trace * 10 + 1; }
    ~SelectionGuard() { *trace = *trace * 10 + 2; }
};
template<class T> int selection_initializer(T value, int *trace) {
    if constexpr (SelectionGuard guard(trace); sizeof(T) == sizeof(int)) {
        *trace = *trace * 10 + 3;
    } else value.missing_member();
    return *trace;
}
static unsigned short selected_once(int *calls, unsigned short value) { ++*calls; return value; }
static int switch_bool(bool value) {
    switch (value) { case false: return 3; case true: return 5; }
    return 0;
}
static int switch_char(char value) {
    switch (value) { case 0: return 7; case 127: return 11; default: return 13; }
}
static int switch_signed_char(signed char value) {
    switch (value) { case -128: return 17; case -1: return 19; case 127: return 23; default: return 29; }
}
static int switch_short(int *calls, unsigned short value) {
    switch (selected_once(calls, value)) { case 0: return 31; case 32768: return 37; case 65535: return 41; default: return 43; }
}
enum class SelectionByte : unsigned char { low = 0, middle = 128, high = 255 };
static int switch_enum(SelectionByte value) {
    switch (value) { case SelectionByte::low: return 47; case SelectionByte::middle: return 53; case SelectionByte::high: return 59; }
    return 61;
}
int main() {
    int trace = 0;
    if (selection_true(2) != 5 || selection_false(2) != 7 || selection_no_else(2) != 9 || selection_nested(2) != 13 ||
        selection_initializer(2, &trace) != 132 || trace != 132) return 1;
    if (switch_bool(false) != 3 || switch_bool(true) != 5 || switch_char(0) != 7 || switch_char(127) != 11 || switch_char(3) != 13 ||
        switch_signed_char(-128) != 17 || switch_signed_char(-1) != 19 || switch_signed_char(127) != 23) return 2;
    int calls = 0;
    if (switch_short(&calls, 0) != 31 || switch_short(&calls, 32768) != 37 || switch_short(&calls, 65535) != 41 ||
        switch_short(&calls, 3) != 43 || calls != 4) return 3;
    if (switch_enum(SelectionByte::low) != 47 || switch_enum(SelectionByte::middle) != 53 ||
        switch_enum(SelectionByte::high) != 59 || switch_enum(static_cast<SelectionByte>(3)) != 61) return 4;
    return 0;
}
