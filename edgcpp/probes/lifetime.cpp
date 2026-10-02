int count;
struct Guard {
    char storage;
    Guard() { count += 2; }
    ~Guard() { count += 3; }
};
int main() {
    { Guard g; if (count != 2) return 1; }
    return count == 5 ? 0 : 2;
}
