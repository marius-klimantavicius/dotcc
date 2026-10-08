struct ConstantPair { int left, right; };
ConstantPair global_pair = { 17, 25 };
static const ConstantPair constant_rows[] = { { 3, 5 }, { 7, 11 } };
static int *global_pointer = &global_pair.right;
static int local_constant() {
    static const ConstantPair local = { 13, 19 };
    return local.left + local.right;
}
int main() {
    return global_pair.left != 17 || *global_pointer != 25 ||
        constant_rows[1].right != 11 || local_constant() != 32;
}
