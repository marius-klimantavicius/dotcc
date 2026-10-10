extern "C" int putchar(int);
static int attempts;
struct ArrayEntry {
    int id;
    ArrayEntry() : id(++attempts) {
        putchar('0' + id);
        if (id == 2) throw 7;
    }
    ~ArrayEntry() { putchar('A' + id); }
};
static int initialize() {
    try {
        static ArrayEntry entries[3];
        return entries[0].id == 3 && entries[2].id == 5 ? 0 : 2;
    } catch (int error) {
        putchar('c');
        return error == 7 ? 1 : 3;
    }
}
int main() {
    if (initialize() != 1 || initialize() != 0) return 1;
    putchar('M');
    return 0;
}
