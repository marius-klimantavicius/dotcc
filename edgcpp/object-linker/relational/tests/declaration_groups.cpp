int trace = 0;

int effect(int value) {
    trace = trace * 10 + value;
    return value;
}

struct Item {
    int value;
    Item(int selected) : value(effect(selected)) {
        if (selected == 5) throw selected;
    }
    ~Item() { trace = trace * 10 + value; }
};

int scalar_group() {
    int first = effect(1), second = effect(2);
    return first * 10 + second;
}

int record_group() {
    Item first(3), second(4);
    return first.value * 10 + second.value;
}

int failed_group() {
    Item first(3), second(5);
    return first.value + second.value;
}

int main() {
    if (scalar_group() != 12 || trace != 12) return 1;
    trace = 0;
    if (record_group() != 34 || trace != 3443) return 2;
    trace = 0;
    try {
        failed_group();
        return 3;
    } catch (int code) {
        if (code != 5 || trace != 353) return 4;
    }
    return 0;
}
