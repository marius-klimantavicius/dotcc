extern int trace;
extern int arguments;
int selected_argument(int value);
struct Value {
    Value *self;
    int value;
    Value(int value, int tag);
    Value(int value);
    Value(int value, bool fail);
};
