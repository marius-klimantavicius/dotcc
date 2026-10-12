int trace;

int selected_value(int value) {
    trace = trace * 10 + value;
    return value;
}

struct Item {
    int value;
    explicit Item(int input) : value(selected_value(input)) {}
};

Item first[] = {Item(1), Item(2)};
int middle = selected_value(3);
Item second[] = {Item(4), Item(5)};

int main() {
    if (trace != 12345) return 1;
    if (first[0].value != 1 || first[1].value != 2 || middle != 3) return 2;
    if (second[0].value != 4 || second[1].value != 5) return 3;
    return 0;
}
