// Compile-only expected-unsupported input. Do not execute termination behavior.
static int destructions;

struct NoexceptCallTracked {
    int id;
    explicit NoexceptCallTracked(int value) noexcept : id(value) { }
    ~NoexceptCallTracked() noexcept { ++destructions; }
};

static void raise_selected(int value)
{
    NoexceptCallTracked nested(3);
    throw value;
}

// This function deliberately has no class locals or cleanup table. Its selected
// noexcept boundary still forbids propagating the generated carrier to a catch.
static void through_noexcept(int value) noexcept
{
    raise_selected(value);
}

static int handler_value(int value) noexcept
{
    return value + destructions;
}

extern "C" int fixture()
{
    NoexceptCallTracked outer(1);
    try {
        NoexceptCallTracked inner(2);
        through_noexcept(7);
        return outer.id + inner.id + destructions;
    } catch (int value) {
        return outer.id + handler_value(value);
    }
}
