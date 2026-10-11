extern int trace;
void mark(int value);

struct ScopeGuard {
    ScopeGuard();
    ~ScopeGuard();
};

struct Result {
    Result *self;
    int value;
    Result(int value, bool fail);
    ~Result();
    bool stable() const;
};

struct Scalar {
    int value;
    Scalar(int value);
};

Result make_result(int value, bool fail);
Result forward_result(int value, bool fail);
Scalar make_scalar(int value);
int exercise(bool fail);
