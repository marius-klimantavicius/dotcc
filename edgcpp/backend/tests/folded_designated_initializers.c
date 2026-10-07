/* EDG can fold assignment from a const record into a ck_aggregate expression
   whose designators survive declaration initializer lowering. */
struct Plain { int first; int middle; int last; };
struct Table {
    const char *name;
    int (*callback)(int);
    union { int cells[3]; long scalar; };
};
struct Nested { int prefix; struct Plain rows[4]; union { int cells[3]; long scalar; } value; };
static int twice(int n) { return n * 2; }
static const struct Plain plain = { .first = 7, .last = 19 };
static const struct Table table = { .name = "table", .callback = twice };
static const struct Nested nested = {
    .prefix = 5,
    .rows = { [1] = { .first = 11, .last = 17 }, [3] = { .last = 43 } }, .value = { .scalar = 91 }
};
static const struct Plain omitted = { .last = 3 };
void assign_plain(struct Plain *out) { *out = plain; }
void assign_table(struct Table *out) { *out = table; }
void assign_nested(struct Nested *out) { *out = nested; }
void assign_omitted(struct Plain *out) { *out = omitted; }
int main(void)
{
    struct Plain p = { 99, 99, 99 };
    struct Table t;
    struct Nested n;
    int i;
    assign_plain(&p);
    if (p.first != 7 || p.middle != 0 || p.last != 19) return 1;
    t.scalar = -1;
    assign_table(&t);
    if (t.name[0] != 't' || t.callback(9) != 18 || t.cells[0] || t.cells[1] || t.cells[2]) return 2;
    assign_nested(&n);
    if (n.prefix != 5 || n.value.scalar != 91 || n.rows[1].first != 11 || n.rows[1].last != 17 || n.rows[3].last != 43) return 3;
    for (i = 0; i < 4; ++i) if (n.rows[i].middle) return 4;
    if (n.rows[0].first || n.rows[0].last || n.rows[2].first || n.rows[2].last || n.rows[3].first) return 5;
    assign_omitted(&p);
    if (p.first != 0 || p.middle != 0 || p.last != 3) return 6;
    p = (struct Plain){ .last = 31, .first = 23 };
    if (p.first != 23 || p.middle != 0 || p.last != 31) return 7;
    return 0;
}
