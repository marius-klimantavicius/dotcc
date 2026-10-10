// EDG's argument and result ABI rules intentionally differ for this class.
// A deleted copy constructor and trivial move permit direct parameter passing,
// while returning the same source type uses caller-provided result storage.
struct ArgumentOnlyTransport {
    int &&value;
    explicit ArgumentOnlyTransport(int &&source) : value(static_cast<int &&>(source)) {}
    ArgumentOnlyTransport(const ArgumentOnlyTransport &) = delete;
    ArgumentOnlyTransport(ArgumentOnlyTransport &&) = default;
};

int consume_argument(ArgumentOnlyTransport value) { return value.value; }

ArgumentOnlyTransport make_argument(int &&value) {
    return ArgumentOnlyTransport(static_cast<int &&>(value));
}

int main() {
    int value = 37;
    if (consume_argument(ArgumentOnlyTransport(static_cast<int &&>(value))) != 37) return 1;
    if (consume_argument(make_argument(static_cast<int &&>(value))) != 37) return 2;
    return 0;
}
