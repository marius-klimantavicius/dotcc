static int nested(int selector) {
    int result = 0;
    switch (selector) {
        result = 999;
        case 1: result += 1;
        {
            case 2: result += 2;
            {
                case 3: result += 4;
            }
            if (result > 0) break;
        }
        default: result = 16; break;
        case 4:
            for (int index = 0; index < 2; ++index) {
                case 5: result += 32;
                break;
            }
            break;
    }
    return result;
}
static int independent(int outer, int inner) {
    int result = 0;
    switch (outer) {
        case 1:
            switch (inner) {
                case 1: { case 2: result = 17; break; }
                default: result = 19;
            }
            break;
        case 2: result = 23; break;
        default: result = 29;
    }
    return result;
}
int main(void) {
    if (nested(1) != 7 || nested(2) != 6 || nested(3) != 4 || nested(0) != 16) return 1;
    if (nested(4) != 32 || nested(5) != 32) return 2;
    if (independent(1, 1) != 17 || independent(1, 2) != 17 || independent(1, 3) != 19) return 3;
    if (independent(2, 2) != 23 || independent(3, 2) != 29) return 4;
    return 0;
}
