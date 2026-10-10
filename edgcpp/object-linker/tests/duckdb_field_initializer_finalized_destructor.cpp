extern "C" int putchar(int);
static int fail;
struct FinalizedValue {
    int id;
    FinalizedValue(const char*);
    ~FinalizedValue();
};
int finish_member() { if (fail) throw 7; return 1; }
struct FinalizedHolder {
    FinalizedValue value = "";
    int second = finish_member();
};
// Force original record metadata before the implicit constructor is selected.
int inspect(FinalizedHolder* holder) { return holder->value.id; }
FinalizedValue::FinalizedValue(const char*) : id(3) { putchar('C'); }
FinalizedValue::~FinalizedValue() { putchar('D'); }
int main() {
    fail = 1;
    try { FinalizedHolder holder; return 1; }
    catch (int error) { if (error != 7) return 2; putchar('!'); }
    fail = 0;
    { FinalizedHolder holder; if (inspect(&holder) != 3) return 3; }
    return 0;
}
