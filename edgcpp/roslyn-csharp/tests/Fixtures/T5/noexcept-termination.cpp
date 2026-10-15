// Borrowed shared-memory protocol: count[0], overflow[1], live[2], result[3],
// events[4..67]. The isolated caller supplies 68 zero-initialized target ints.
// Source execution makes no native calls; the parent reads after child exit.
static int* observations;

static void note(int event) noexcept
{
    int index = observations[0];
    if (index >= 0 && index < 64) {
        observations[4 + index] = event;
        observations[0] = index + 1;
    } else observations[1] = 1;
}

struct TerminationTracked {
    int id;
    explicit TerminationTracked(int value) noexcept : id(value)
    {
        ++observations[2];
        note(100 + id);
    }
    ~TerminationTracked() noexcept
    {
        note(300 + id);
        --observations[2];
    }
};

static void raise_selected(int raising)
{
    TerminationTracked leaf(2);
    note(507);
    if (raising != 0) throw 7;
    note(508);
}

static void boundary(int raising) noexcept
{
    TerminationTracked at_boundary(3);
    note(800);
    raise_selected(raising);
    note(801);
}

extern "C" int fixture(int* buffer, int raising)
{
    observations = buffer;
    TerminationTracked outer(1);
    try {
        boundary(raising);
        note(900);
        observations[3] = 1;
        return 1;
    } catch (int value) {
        note(700 + value);
        observations[3] = -value;
        return -value;
    }
}
