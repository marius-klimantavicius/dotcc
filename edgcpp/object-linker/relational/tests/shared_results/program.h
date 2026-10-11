extern long long trace;
extern int alive, constructions, copies, destructions, selections, copy_failure;
void mark(int digit);
void reset();

struct Guard {
    int event;
    Guard(int event);
    ~Guard() noexcept;
};

struct Value {
    Value *self;
    int value;
    Value(int input, bool fail);
    Value(const Value &other);
    ~Value() noexcept;
    bool stable() const;
};

Value make_value(int value, bool fail);
Value choose_result(bool fresh, bool fail, bool fail_copy);
Value choose_comma(bool fresh, bool fail, bool fail_copy);
Value choose_nested(bool outer, bool inner, bool fail);
int local_result(bool fresh, bool fail, bool fail_copy);
