// Actual abstract and deleted virtual slots. The invalid call branches are
// explicit ABI fatal-path tests and run only in isolated subprocesses.
extern "C" void __cxa_pure_virtual();
extern "C" void __cxa_deleted_virtual();
void operator delete(void*) noexcept {}
void operator delete(void*, __SIZE_TYPE__) noexcept {}
static int destroyed;
struct Abstract {
    virtual ~Abstract() { destroyed += 1; }
    virtual int read() const = 0;
    virtual void removed() = delete;
};
struct Implementation : Abstract {
    int value;
    explicit Implementation(int n) : value(n) {}
    ~Implementation() override { destroyed += 10; }
    int read() const override { return value + 1; }
};
static int through_interface(const Abstract* value) { return value->read(); }
extern "C" void fatal_pure() { __cxa_pure_virtual(); }
extern "C" void fatal_deleted() { __cxa_deleted_virtual(); }
int main() {
    {
        Implementation value(41);
        if (through_interface(&value) != 42) return 1;
        value.value = 57;
        if (through_interface(&value) != 58) return 2;
    }
    return destroyed == 11 ? 0 : 3;
}
