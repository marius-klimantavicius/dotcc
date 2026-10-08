/* C++ lifetime acceptance: full-expression and extended temporaries, control
   exits, partial construction, nested rethrow, and array allocation failure.
   Trace checks avoid unspecified argument-evaluation and copy-elision order. */
static int trace[64], used, live;
static void record(int id) { trace[used++] = id; }
struct Failure { int id; };
struct Guard {
    int id;
    explicit Guard(int value) : id(value) {
        record(id);
        if (id == 13) throw Failure{id};
        ++live;
    }
    Guard(const Guard &other) : id(other.id) { record(id); ++live; }
    ~Guard() { record(-id); --live; }
};
static bool same(const int *expected, int length)
{
    if (used != length || live != 0) return false;
    for (int i = 0; i < length; ++i) if (trace[i] != expected[i]) return false;
    return true;
}
static int lexical_return()
{
    Guard outer(1);
    { Guard inner(2); return 17; }
}
static int consume(const Guard &value) { return value.id + live; }
struct Pair {
    Guard first, second;
    Pair() : first(1), second(13) { record(99); }
    ~Pair() { record(-99); }
};
static int next_array;
struct Element {
    int id;
    Element() : id(++next_array) {
        record(id);
        if (id == 3) throw Failure{3};
        ++live;
    }
    ~Element() { record(-id); --live; }
};

int main()
{
    used = 0;
    if (lexical_return() != 17) return 1;
    const int returned[] = {1, 2, -2, -1};
    if (!same(returned, 4)) return 2;

    used = 0;
    for (int i = 0; i != 3; ++i) {
        Guard guard(10 + i);
        if (i == 0) continue;
        break;
    }
    const int loop[] = {10, -10, 11, -11};
    if (!same(loop, 4)) return 3;

    used = 0;
    { Guard outer(1); { Guard inner(2); goto after; } }
  after:
    if (!same(returned, 4)) return 4;

    used = 0;
    if (consume(Guard(20)) != 21) return 5;
    const int temporary[] = {20, -20};
    if (!same(temporary, 2)) return 6;
    used = 0;
    volatile bool select = true;
    {
        const Guard &extended = select ? Guard(30) : Guard(31);
        if (live != 1 || used != 1 || extended.id != 30) return 7;
    }
    const int extended[] = {30, -30};
    if (!same(extended, 2)) return 8;

    used = 0;
    try { Pair pair; return 9; }
    catch (const Failure &error) { if (error.id != 13) return 10; }
    const int partial[] = {1, 13, -1};
    if (!same(partial, 3)) return 11;

    used = 0;
    try {
        Guard outer(1);
        try { Guard inner(2); throw Failure{42}; }
        catch (const Failure &error) {
            if (error.id != 42 || live != 1) return 12;
            record(7);
            throw;
        }
    }
    catch (const Failure &error) { if (error.id != 42) return 13; record(8); }
    const int rethrow[] = {1, 2, -2, 7, -1, 8};
    if (!same(rethrow, 6)) return 14;

    used = 0;
    try { Element *elements = new Element[4]; delete[] elements; return 15; }
    catch (const Failure &error) { if (error.id != 3) return 16; }
    const int array[] = {1, 2, 3, -2, -1};
    if (!same(array, 5)) return 17;
    return 0;
}
