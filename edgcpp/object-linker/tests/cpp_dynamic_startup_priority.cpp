// GNU init_priority reverses source order; owned scheduling preserves it.
int startup_trace;
struct PriorityValue {
    int value;
    PriorityValue(int digit) : value(digit) {
        startup_trace = startup_trace * 10 + digit;
    }
};
PriorityValue later __attribute__((init_priority(200))) (2);
PriorityValue earlier __attribute__((init_priority(101))) (1);
int main() {
    return startup_trace != 12 || earlier.value != 1 || later.value != 2;
}
