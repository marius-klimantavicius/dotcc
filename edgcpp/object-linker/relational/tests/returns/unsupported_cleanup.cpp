struct ThrowingCleanup {
    ThrowingCleanup() {}
    ~ThrowingCleanup() noexcept(false) { throw 9; }
};
struct Returned {
    Returned() {}
    ~Returned() {}
};
Returned unsupported_return() {
    ThrowingCleanup cleanup;
    return Returned();
}
int main() { return 0; }
