struct Cell { int number; unsigned char flag; };
union Word { unsigned int bits; unsigned char bytes[4]; };
struct Row {
    struct Cell cells[3];
    char name[6];
    union Word value;
    unsigned int : 3;
    unsigned int flag : 5;
};
static int probe(void) {
    struct Row rows[3] = {
        [2] = { .cells = { [1] = { 37, 211 } }, .name = "third", .value = { .bits = 0x12345678U }, .flag = 19 },
        [0] = { .cells = { { 13, 17 }, { 23, 31 } }, .name = "first", .value = { .bytes = { 9, 8, 7, 6 } }, .flag = 7 }
    };
    if (rows[0].cells[0].number != 13 || rows[0].cells[1].flag != 31 || rows[0].cells[2].number != 0) return 1;
    if (rows[1].cells[1].number || rows[1].name[0] || rows[1].value.bits || rows[1].flag) return 2;
    if (rows[2].cells[1].number != 37 || rows[2].cells[1].flag != 211 || rows[2].cells[0].flag) return 3;
    if (rows[0].name[4] != 't' || rows[0].name[5] || rows[2].name[0] != 't') return 4;
    if (rows[0].value.bits != 0x06070809U || rows[2].value.bytes[3] != 0x12 || rows[2].flag != 19) return 5;
    struct Cell matrix[2][3] = { { { 1, 2 } }, { [2] = { 3, 4 } } };
    if (matrix[0][0].number != 1 || matrix[0][1].number || matrix[1][2].flag != 4) return 6;
    union Word word = { .bytes = { 4, 3 } };
    if (word.bits != 0x0304U) return 7;
    return 0;
}
int main(void) { return probe(); }
