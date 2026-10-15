/* Same source spellings across TUs are deliberately insufficient identities. */
namespace captured {
int choose(int value);
unsigned choose(unsigned value);
}

static int local_helper(int value) { return value + 5; }
extern "C" int unit_a(int value);

extern "C" int fixture(void) {
    return unit_a(4) + captured::choose(7) + (int)captured::choose(9u)
        + local_helper(3);
}
