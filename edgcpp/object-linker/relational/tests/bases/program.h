extern int trace;
void mark(int value);

struct Left {
    int left;
    Left(int value);
    ~Left();
    int read_left() const;
};

struct Right {
    int right;
    Right(int value, bool fail);
    ~Right();
    int read_right() const;
};

struct Derived : Left, Right {
    int value;
    Derived(bool fail);
    ~Derived();
};

int exercise(bool fail);
int add_right(Right *value, int amount);
