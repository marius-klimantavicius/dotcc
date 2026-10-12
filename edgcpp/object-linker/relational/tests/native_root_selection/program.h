inline int called_helper(int value) { return value + 3; }
inline int address_helper(int value) { return value * 2; }
inline int unused_body(int value) { return value + 91; }
struct Guard {
    int *counter;
    Guard(int *counter) : counter(counter) { ++*counter; }
    ~Guard() noexcept { ++*counter; }
};
extern int (*selected_address)(int);
int provider(int value, int *counter);
