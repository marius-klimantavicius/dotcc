struct Later;
Later *identity(Later *p) { return p; }
struct Later {
    int value;
    Later(int x) : value(x) {}
    int read() const { return value; }
};
int exercise() {
    Later value(7);
    return identity(&value)->read();
}
