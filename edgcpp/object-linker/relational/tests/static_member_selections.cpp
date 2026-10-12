int trace, receiver_calls, argument_calls;
struct Receiver {
    int marker;
    static int value;
    static int event(int input);
};
Receiver object = {7};
int Receiver::value = 9;
int Receiver::event(int input) { trace = trace * 10 + 3; return input + value; }
Receiver *select_pointer() { ++receiver_calls; trace = trace * 10 + 2; return &object; }
Receiver &select_reference() { ++receiver_calls; trace = trace * 10 + 2; return object; }
int argument() { ++argument_calls; trace = trace * 10 + 1; return 5; }
int pointer_call() { return select_pointer()->event(argument()); }
int reference_call() { return select_reference().event(argument()); }
int &pointer_value() { return select_pointer()->value; }
int &reference_value() { return select_reference().value; }
void assign_value() { select_pointer()->value = argument(); }
void reset() { trace = receiver_calls = argument_calls = 0; Receiver::value = 9; }
int main() {
    reset();
    if (pointer_call() != 14 || trace != 213 || receiver_calls != 1 || argument_calls != 1) return 1;
    reset();
    if (reference_call() != 14 || trace != 213 || receiver_calls != 1 || argument_calls != 1) return 2;
    reset();
    int &first = pointer_value();
    first = 17;
    if (Receiver::value != 17 || trace != 2 || receiver_calls != 1) return 3;
    reset();
    int &second = reference_value();
    second = 19;
    if (Receiver::value != 19 || trace != 2 || receiver_calls != 1) return 4;
    reset(); assign_value();
    if (Receiver::value != 5 || trace != 12 || receiver_calls != 1 || argument_calls != 1) return 5;
    return 0;
}
