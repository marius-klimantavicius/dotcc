int scopes(int value) {
    int result = 0;
    if (value > 0) {
        int value = 3;
        result = value;
    } else {
        int value = 4;
        result = value;
    }
    return result;
}

int short_circuit(int value) {
    int state = 0;
    if (value && (state = 9)) return state;
    return state;
}

int repeated_initialization(int count) {
    int sum = 0;
    while (count > 0) {
        int current = count;
        sum = sum + current;
        count = count - 1;
    }
    return sum;
}

int main() {
    if (scopes(1) != 3) return 1;
    if (scopes(0) != 4) return 2;
    if (short_circuit(0) != 0) return 3;
    if (short_circuit(1) != 9) return 4;
    if (repeated_initialization(3) != 6) return 5;
    return 0;
}
