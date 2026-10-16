// Capture-first candidate: layout, adjustments and observations are pending.
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

struct Left {
    int left_id;

    explicit Left(int value) noexcept : left_id(value)
    {
        ++live_count;
        note(100 + left_id);
    }

    virtual int evaluate_left(int argument) noexcept
    {
        note(600 + left_id);
        return left_id + argument;
    }

    ~Left() noexcept
    {
        note(300 + left_id);
        --live_count;
    }
};

struct Right {
    int right_id;

    explicit Right(int value) noexcept : right_id(value)
    {
        ++live_count;
        note(200 + right_id);
    }

    virtual int evaluate_right(int argument) noexcept
    {
        note(700 + right_id);
        return right_id + argument * 2;
    }

    ~Right() noexcept
    {
        note(400 + right_id);
        --live_count;
    }
};

struct Derived : Left, Right {
    int bonus;

    Derived(int left, int right, int extra) noexcept
        : Left(left), Right(right), bonus(extra)
    {
        ++live_count;
        note(500 + bonus);
    }

    int evaluate_left(int argument) noexcept override
    {
        note(800 + left_id);
        return left_id + right_id + bonus + argument;
    }

    int evaluate_right(int argument) noexcept override
    {
        note(900 + right_id);
        return left_id + right_id + bonus + argument * 2;
    }

    ~Derived() noexcept
    {
        note(1000 + left_id + right_id + bonus);
        --live_count;
    }
};

static int dispatch_left(Left* receiver, int argument) noexcept
{
    note(1100);
    return receiver->evaluate_left(argument);
}

static int dispatch_right(Right* receiver, int argument) noexcept
{
    note(1200);
    return receiver->evaluate_right(argument);
}

static int ordinary_path() noexcept
{
    Left left(2);
    Right right(3);
    Derived derived(5, 7, 11);
    Left* left_view = &derived;
    Right* right_view = &derived;
    values[0] = dispatch_left(&left, 3);
    values[1] = dispatch_right(&right, 4);
    values[2] = dispatch_left(left_view, 13);
    values[3] = dispatch_right(right_view, 17);
    values[4] = left_view->left_id;
    values[5] = right_view->right_id;
    values[6] = derived.bonus;
    return values[0] + values[1] + values[2] + values[3];
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = live_count;
    int result = ordinary_path();
    values[7] = live_count;
    return result;
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
