int &select_reference(int &value) { return value; }
int read_reference(const int &value) { return value; }
int consume_reference(int &&value) { return value + 1; }
struct Counter {
    int value = 4;
    int get() const & { return value; }
    int add(int &delta) & { value += delta; return value; }
    int take() && { return value; }
    int volatile_get() volatile & { return value; }
    static int twice(int value) { return 2 * value; }
};
int main() {
    Counter counter{3};
    Counter *pointer = &counter;
    int delta = 5;
    if (pointer->add(delta) != 8 || counter.get() != 8) return 1;
    int &alias = select_reference(counter.value);
    alias = 9;
    if (counter.get() != 9 || Counter::twice(delta) != 10) return 2;
    if (consume_reference(12) != 13 || read_reference(17) != 17) return 3;
    if (static_cast<Counter&&>(counter).take() != 9) return 4;
    int &(*selector)(int &) = select_reference;
    selector(delta) = 7;
    if (delta != 7) return 5;
    volatile Counter *volatile_pointer = &counter;
    if (volatile_pointer->volatile_get() != 9) return 6;
    return 0;
}
