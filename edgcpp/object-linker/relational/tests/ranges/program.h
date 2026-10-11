extern int alive;
extern int expected_alive;
extern int bad_steps;
extern int begins;
extern int ends;
struct Watch { int value; Watch(int input); ~Watch(); };
struct Sentinel { int *finish; Sentinel(int *value); };
struct Cursor {
    int *pointer;
    Cursor(int *value);
    int &operator*() const;
    Cursor &operator++();
    bool operator!=(const Sentinel &other) const;
};
struct Range { int *values; int count; Range(int *data, int size); Cursor begin() const; Sentinel end() const; };
int early_return(Range &range);
