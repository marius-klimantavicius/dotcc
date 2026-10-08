/* Portable LP64 C++ object/subobject acceptance gate. No standard library.
   A complete TailBase has stride8, but its live base extent is only5. */
static int trace[32], used;
static void record(int value) { trace[used++] = value; }

struct TailBase {
    int value;
    char marker;
    TailBase(int v = 11, char m = 'b') : value(v), marker(m) {}
};
struct TailDerived : TailBase {
    char tail;
    TailDerived() : TailBase(), tail('d') {}
};
static_assert(sizeof(TailBase) == 8, "portable complete base size");
static_assert(sizeof(TailDerived) == 8, "base tail must be reusable");
static int tail_layout()
{
    TailDerived values[2];
    if ((char *)&values[0].tail - (char *)(TailBase *)&values[0] != 5) return 1;
    if ((char *)&values[1] - (char *)&values[0] != sizeof(TailDerived)) return 2;
    TailBase source(41, 'x');
    static_cast<TailBase &>(values[0]) = source;
    if (values[0].value != 41 || values[0].marker != 'x' || values[0].tail != 'd') return 3;
    TailDerived copy = values[0];
    if (copy.value != 41 || copy.tail != 'd') return 4;
    TailBase complete[2];
    if ((char *)&complete[1] - (char *)&complete[0] != 8) return 5;
    return 0;
}

struct Left {
    int left;
    Left() : left(17) {}
    virtual int read_left() const { return left; }
    virtual ~Left() { record(-1); }
};
struct Right {
    int right;
    Right() : right(23) {}
    virtual int read_right() const { return right; }
    virtual ~Right() { record(-2); }
};
struct Both : Left, Right {
    int extra;
    Both() : extra(5) {}
    int read_left() const override { return left + extra; }
    int read_right() const override { return right + extra; }
    ~Both() override { record(-3); }
};
__attribute__((noinline)) static int call_left(const Left *p) { return p->read_left(); }
__attribute__((noinline)) static int call_right(const Right *p) { return p->read_right(); }
static int multiple_layout()
{
    used = 0;
    {
        Both value;
        Left *left = &value;
        Right *right = &value;
        if ((char *)right <= (char *)left) return 10;
        if (static_cast<Both *>(right) != &value || static_cast<Both *>(left) != &value) return 11;
        if (call_left(left) != 22 || call_right(right) != 28) return 12;
        if (left->left != 17 || right->right != 23) return 13;
    }
    if (used != 3 || trace[0] != -3 || trace[1] != -2 || trace[2] != -1) return 14;
    return 0;
}

struct Root {
    int root;
    Root() : root(31) { record(1); }
    virtual int read() const { return root; }
    virtual ~Root() { record(-1); }
};
struct BranchA : virtual Root {
    BranchA() { record(2); }
    ~BranchA() override { record(-2); }
};
struct BranchB : virtual Root {
    BranchB() { record(3); }
    ~BranchB() override { record(-3); }
};
struct Diamond : BranchA, BranchB {
    int leaf;
    Diamond() : leaf(7) { record(4); }
    int read() const override { return root + leaf; }
    ~Diamond() override { record(-4); }
};
__attribute__((noinline)) static int call_root(const Root *p) { return p->read(); }
static int virtual_layout()
{
    used = 0;
    {
        Diamond value;
        BranchA *a = &value;
        BranchB *b = &value;
        Root *first = a, *second = b;
        if (first != second || first != static_cast<Root *>(&value)) return 20;
        if (call_root(first) != 38 || call_root(second) != 38) return 21;
        if (used != 4) return 22;
        for (int i = 0; i < 4; ++i) if (trace[i] != i + 1) return 23;
    }
    if (used != 8) return 24;
    for (int i = 0; i < 4; ++i) if (trace[i + 4] != i - 4) return 25;
    return 0;
}

int main()
{
    int rc = tail_layout();
    if (rc) return rc;
    rc = multiple_layout();
    if (rc) return rc;
    return virtual_layout();
}
