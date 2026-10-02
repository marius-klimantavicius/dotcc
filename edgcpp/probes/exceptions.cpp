int destroyed;
struct Guard { ~Guard() { ++destroyed; } };
void fail() { Guard g; throw 42; }
int main() {
    try { fail(); } catch (int x) { return x == 42 && destroyed == 1 ? 0 : 1; }
    return 2;
}
