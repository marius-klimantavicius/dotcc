extern int trace;
extern int cleaned;
extern bool fail_receiver;
extern bool fail_call;
extern bool fail_payload;
void append(int event);
struct Input {
    int value;
    Input *self;
    Input(int value, bool fail);
    ~Input() noexcept;
};
struct Accumulator {
    int value;
    Accumulator &operator+=(const Input &input);
};
struct Error {
    int value;
    Error(const Input &input);
};
extern Accumulator accumulator;
Accumulator &get_accumulator();
void discard_record(bool fail_input);
Accumulator *scoped_address();
void throw_scoped(bool fail_input);
