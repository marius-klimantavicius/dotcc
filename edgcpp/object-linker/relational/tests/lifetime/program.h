extern int trace;
void mark(int value);

struct Failure {
    int code;
    Failure(int value);
    ~Failure();
};

struct Guard {
    int value;
    Guard(int value, bool fail);
    ~Guard();
};

struct Pair {
    Guard first;
    Guard second;
    Pair(bool fail);
    ~Pair();
};

int exercise(bool fail);
