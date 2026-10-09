// Trivial-destructor temporaries exercise observed startup scope scaffolding.
int trace;
struct Input {
    int value;
    Input(int input) : value(input) { trace = trace * 10 + 1; }
};
int extract(Input input) { return input.value; }
int initialized = extract(Input(7));
int main() { return initialized != 7 || trace != 1; }
