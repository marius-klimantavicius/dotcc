namespace First { int same(int value) { return value + 1; } }
namespace Second { int same(int value) { return value + 2; } }

bool truth(int value) { return (bool)value; }
unsigned char narrow(int value) { return (unsigned char)value; }
int owner_shadow(int Core) { return First::same(Core); }

int main() {
    if (!truth(256)) return 1;
    if (!truth(-256)) return 2;
    if (truth(0)) return 3;
    if (narrow(256) != 0) return 4;
    if (narrow(-1) != 255) return 5;
    if (First::same(1) != 2) return 6;
    if (Second::same(1) != 3) return 7;
    if (owner_shadow(2) != 3) return 8;
    return 0;
}
