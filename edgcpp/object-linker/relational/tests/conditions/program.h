struct Watch {
    int *alive;
    int value;
    Watch(int *count, int input);
    ~Watch();
    explicit operator bool() const;
};
int *next_pointer(int *value, int *remaining);
void step(int *index, int *alive, int *bad);
int exercise();
