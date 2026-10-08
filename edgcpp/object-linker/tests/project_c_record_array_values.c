struct Cell { int number; unsigned char flag; };
struct Bundle {
    int matrix[2][3];
    int *pointers[2];
    int (*callbacks[2])(int);
    struct Cell cells[2];
};
struct Bundle shared_bundle;
static int add(int value) { return value + 17; }
static int subtract(int value) { return value - 9; }

struct Bundle make_bundle(int *values) {
    struct Bundle result;
    for (int row = 0; row < 2; ++row)
        for (int column = 0; column < 3; ++column)
            result.matrix[row][column] = row * 10 + column;
    result.pointers[0] = values;
    result.pointers[1] = values + 1;
    result.callbacks[0] = add;
    result.callbacks[1] = subtract;
    result.cells[0].number = 37;
    result.cells[0].flag = 211;
    result.cells[1].number = 71;
    result.cells[1].flag = 199;
    return result;
}
struct Bundle bounce_bundle(struct Bundle value) {
    value.matrix[1][2] += 5;
    value.cells[1].number += 6;
    return value;
}
int inspect_bundle(struct Bundle value) {
    if (value.matrix[0][0] != 0 || value.matrix[0][2] != 2 || value.matrix[1][2] != 12) return 1;
    if (*value.pointers[0] != 41 || *value.pointers[1] != 43) return 2;
    if (value.callbacks[0](5) != 22 || value.callbacks[1](12) != 3) return 3;
    if (value.cells[0].number != 37 || value.cells[0].flag != 211) return 4;
    if (value.cells[1].number != 71 || value.cells[1].flag != 199) return 5;
    return 0;
}
int main(void) {
    int values[2] = { 41, 43 };
    struct Bundle first = make_bundle(values);
    struct Bundle second = first;
    shared_bundle = second;
    if (inspect_bundle(first) || inspect_bundle(second) || inspect_bundle(shared_bundle)) return 1;
    struct Bundle changed = bounce_bundle(first);
    if (changed.matrix[1][2] != 17 || changed.cells[1].number != 77) return 2;
    if (inspect_bundle(first) || inspect_bundle(second) || inspect_bundle(shared_bundle)) return 3;
    if (changed.cells[0].flag != 211 || changed.cells[1].flag != 199 || changed.callbacks[0](1) != 18) return 4;
    return 0;
}
