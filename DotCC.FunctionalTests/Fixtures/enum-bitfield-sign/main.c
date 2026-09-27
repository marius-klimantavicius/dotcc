#include <stdio.h>
typedef enum { ZERO, HIGH = 7 } Positive;
enum Negative { MINUS = -4, PLUS = 3 };
struct Fields {
    Positive closure_type : 3;
    unsigned int flag : 1;
    enum Negative signed_type : 3;
};
int main(void) {
    struct Fields f = {0};
    int sum = 0;
    for (int i = 0; i < 8; i++) {
        f.closure_type = (Positive)i;
        f.flag = 1;
        f.signed_type = MINUS;
        if ((int)f.closure_type != i || f.signed_type != -4 || f.flag != 1) return 1;
        sum += f.closure_type;
    }
    f.signed_type = PLUS;
    printf("enum bitfields %d %d %d %d\n", sum, (int)f.closure_type, (int)f.signed_type, ZERO - 1);
    return 0;
}
