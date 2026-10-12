int source_mask(int);
int retained_helpers();
int source_product(int, int, int);
unsigned source_wrap(unsigned, unsigned);
int temporary_assignment();
int main() {
    if (source_mask(31) != 20 || source_mask(2) != 17) return 5;
    if (source_product(5, 3, 1) != 8) return 6;
    if (temporary_assignment() != 0) return 7;
    if (source_wrap(~0u, 2u) != 0xfffffffeu) return 8;
    return retained_helpers();
}
