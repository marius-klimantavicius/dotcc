struct Guard {
    ~Guard() { }
};
int main() {
    { Guard guard; }
    return 0;
}
