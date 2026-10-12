struct External {
    int stored;
    External(int input) noexcept : stored(input) {}
    virtual int eval(int add) noexcept;
};

extern "C" void *provided_table_address() noexcept;
void *constructed_table_address(int input) noexcept;
int dispatch(External *value, int add) noexcept;
