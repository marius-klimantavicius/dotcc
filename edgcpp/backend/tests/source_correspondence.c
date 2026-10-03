/* Read this beside its generated C#: ordinary names, expressions and control
   flow should remain recognizable without numbered assignment helpers. */
typedef struct Item {
    int value;
    unsigned weight;
} Item;

typedef struct Flags {
    unsigned ready : 1;
    unsigned : 2;
    unsigned mode : 3;
    signed delta : 4;
} Flags;

enum Mode { ModeSkip = 0, ModeAdd = 1, ModeDouble = 2 };
static int adjustment = 3;

struct KeywordNames { int event; int ref; };
int keyword_names(int params, int out)
{
    struct KeywordNames names = {params, out};
    int lock = names.event + names.ref;
    return lock;
}

int shadowing(int value)
{
    int total = value;
    {
        int value = 5;
        total += value;
    }
    return total + value;
}

struct same_name { int number; };
int same_name(int number)
{
    struct same_name item = {number};
    return item.number;
}

static int contribution(const Item *item, enum Mode mode)
{
    switch (mode) {
    case ModeSkip:
        return 0;
    case ModeAdd:
        return item->value;
    default:
        return item->value * 2;
    }
}

int sum(const Item *items, int count)
{
    int total = 0;
    for (int index = 0; index < count; ++index)
        total += items[index].value;
    return total;
}

void update_flags(Flags *flags)
{
    flags->ready = 1;
    flags->mode += 2;
    flags->delta = -3;
}

int control_flow(const Item *items, int count)
{
    int total = 0;
    for (int index = 0; index < count; ++index) {
        if (items[index].value < 0)
            continue;
        if (items[index].weight == 0)
            total += contribution(&items[index], ModeAdd);
        else
            total += contribution(&items[index], ModeDouble);
    }
    int remaining = count;
    while (remaining > 0) {
        --remaining;
        if (remaining == 1)
            break;
        total += adjustment;
    }
    do {
        total -= 1;
        ++remaining;
    } while (remaining < count);
    return total;
}

/* Consumed bitfield assignment results must reflect the stored value;
   property setter syntax alone cannot supply that result in C#. */
int bitfield_result(Flags *flags)
{
    return flags->mode = 11;
}

int main(void)
{
    Item items[] = {{4, 0}, {-7, 1}, {5, 2}, {9, 0}};
    Flags flags = {0};
    if (sum(items, 4) != 11)
        return 1;
    if (control_flow(items, 4) != 26)
        return 2;
    update_flags(&flags);
    if (!flags.ready || flags.mode != 2 || flags.delta != -3)
        return 3;
    if (bitfield_result(&flags) != 3 || flags.mode != 3)
        return 4;
    if (keyword_names(7, 9) != 16 || shadowing(3) != 11 || same_name(17) != 17)
        return 5;
    return 0;
}
