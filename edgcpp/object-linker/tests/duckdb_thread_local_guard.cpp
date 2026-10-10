thread_local int attempts;
int initialize() {
    if (++attempts == 1) throw 19;
    return 73;
}
int &cached() {
    static thread_local int value = initialize();
    return value;
}
int main() {
    try { (void)cached(); return 1; }
    catch (int error) { if (error != 19) return 2; }
    int *first = &cached();
    if (*first != 73 || attempts != 2) return 3;
    *first = 81;
    if (&cached() != first || cached() != 81 || attempts != 2) return 4;
    return 0;
}
