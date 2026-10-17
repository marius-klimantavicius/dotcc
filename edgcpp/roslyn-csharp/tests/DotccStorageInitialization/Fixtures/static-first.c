extern int peer_value;
extern int *peer_reference;
int process_zero;
int process_value = 29;
int *process_reference = &peer_value;
int *process_self = &process_value;
int **process_chain = &peer_reference;
int equal_first = 5;
int equal_second = 5;
_Alignas(64) int aligned_value = 67;
int process_array[4] = { 3, 5, 7, 11 };
int *process_element = &process_array[2];
static int local_value = 37;
char *process_literal = "t\0ls";

int *first_local_address(void) { return &local_value; }
int *second_local_address(void);
int static_observe(int index) {
    if (index == 0) return process_zero;
    if (index == 1) return process_value;
    if (index == 2) return *process_reference;
    if (index == 3) return *process_self;
    if (index == 4) return **process_chain;
    if (index == 5) return *process_element;
    if (index == 6) return process_element == &process_array[2];
    if (index == 7) return &equal_first != &equal_second;
    if (index == 8) return ((unsigned long)&aligned_value % 64) == 0;
    if (index == 9) return *first_local_address();
    if (index == 10) return *second_local_address();
    if (index == 11) return first_local_address() != second_local_address();
    if (index == 12) return process_literal[0];
    if (index == 13) return process_literal[1];
    if (index == 14) return process_literal[2];
    return process_literal[4];
}
void static_change(int value) {
    process_zero = value;
    process_array[2] += value;
    equal_first += value;
}
int *static_address(int index) {
    if (index == 0) return &process_zero;
    if (index == 1) return &process_value;
    if (index == 2) return &peer_value;
    if (index == 3) return &aligned_value;
    if (index == 4) return process_element;
    if (index == 5) return &equal_first;
    return &equal_second;
}
