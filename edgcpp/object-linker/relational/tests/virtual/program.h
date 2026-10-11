struct Base {
    int *trace;
    Base(int *output);
    ~Base();
    virtual int value();
    virtual int plus(int operand);
    virtual int additional() = 0;
};
struct Derived : Base {
    int answer;
    Derived(int *output);
    ~Derived();
    int value() override;
    int plus(int operand) override;
    int additional() override;
};
int selected_value(Base *object);
struct Left {
    int left;
    Left(int input);
    virtual int read_left();
};
struct Right {
    int right;
    Right(int input);
    virtual int read_right();
};
struct Both : Left, Right {
    int result;
    Both();
    int read_left() override;
    int read_right() override;
};
int selected_right(Right *object);
int selected_additional(Base *object);
Base *selected_once(Base *object, int *count);
