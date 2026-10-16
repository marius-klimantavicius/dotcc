// Capture-first candidate: VTT roles, layout and observations are pending.
static int events[64];
static int event_count;
static int event_overflow;
static int live_count;
static int values[9];

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct Root;
static int dispatch(Root* receiver, int argument) noexcept;

struct Root {
    int id;

    explicit Root(int value) noexcept : id(value)
    {
        ++live_count;
        note(100 + id);
    }

    virtual int evaluate(int argument) noexcept
    {
        note(700 + id);
        return id + argument;
    }

    ~Root() noexcept
    {
        note(600 + id);
        --live_count;
    }
};

struct Branch : virtual Root {
    int branch_id;

    Branch(int root_value, int branch_value) noexcept
        : Root(root_value), branch_id(branch_value)
    {
        ++live_count;
        note(200 + branch_id);
        values[0] = dispatch(this, 3);
    }

    int evaluate(int argument) noexcept override
    {
        note(800 + branch_id);
        return id + branch_id + argument;
    }

    ~Branch() noexcept
    {
        note(500 + branch_id);
        values[3] = dispatch(this, 19);
        --live_count;
    }
};

struct Final : Branch {
    long long bonus;

    Final(int root_value, int branch_value, long long extra) noexcept
        : Root(root_value), Branch(root_value, branch_value), bonus(extra)
    {
        ++live_count;
        note(300 + static_cast<int>(bonus));
    }

    int evaluate(int argument) noexcept override
    {
        note(900 + static_cast<int>(bonus));
        return id + branch_id + static_cast<int>(bonus) + argument;
    }

    ~Final() noexcept
    {
        note(400 + static_cast<int>(bonus));
        values[2] = dispatch(this, 17);
        --live_count;
    }
};

static int dispatch(Root* receiver, int argument) noexcept
{
    note(1000);
    return receiver->evaluate(argument);
}

static void ordinary_path() noexcept
{
    Final value(5, 7, 11LL);
    Root* root_view = &value;
    values[4] = root_view->id;
    values[5] = value.branch_id;
    values[6] = static_cast<int>(value.bonus);
    values[1] = dispatch(root_view, 13);
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = live_count;
    ordinary_path();
    values[7] = live_count;
    return values[0] + values[1] + values[2] + values[3];
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
extern "C" int lifetime_live() { return live_count; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 9 ? values[index] : -1;
}
