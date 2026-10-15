// Compile-only expected-unsupported input. Do not execute its escaping throw.
static int destructions;

struct HandlerCallTracked {
    int id;
    explicit HandlerCallTracked(int value) noexcept : id(value) { }
    ~HandlerCallTracked() noexcept { ++destructions; }
};

static void raise_selected(int value)
{
    HandlerCallTracked nested(3);
    throw value; // The only source throw; the handler calls this same helper.
}

extern "C" int fixture()
{
    HandlerCallTracked outer(1);
    try {
        HandlerCallTracked inner(2);
        raise_selected(7);
        return outer.id + inner.id + destructions;
    } catch (int value) {
        raise_selected(value + 1); // New throw while the previous owner is caught.
        return outer.id + value + destructions;
    }
}
