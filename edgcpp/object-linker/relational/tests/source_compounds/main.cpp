int source_mask(int);
int retained_helpers();
int main() {
    if (source_mask(31) != 20 || source_mask(2) != 17) return 5;
    return retained_helpers();
}
