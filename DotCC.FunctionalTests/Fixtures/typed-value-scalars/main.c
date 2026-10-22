#include <stdio.h>
#include <stdbool.h>

enum ScanDirection { BACKWARD = -1, STATIONARY = 0, FORWARD = 1 };
static int touches;
static unsigned char *byte_at(unsigned char *p) { touches++; return p; }
static bool truth(bool value) { return value; }
static int scan(enum ScanDirection direction) { return (int)direction; }
static enum ScanDirection reverse(enum ScanDirection direction) { return -direction; }

int main(void) {
    unsigned char bytes[3] = { 0, 2, 255 };
    bool zero = (bool)*byte_at(bytes);
    bool two = (bool)*byte_at(bytes + 1);
    bool last = (bool)*byte_at(bytes + 2);
    if (zero || !two || !last || touches != 3) return 1;
    double yes = (double)truth(two);
    float no = (float)truth(zero);
    long double extended = (long double)truth(last);
    if (yes != 1.0 || no != 0.0 || extended != 1.0L) return 2;
    if ((double)(touches == 3) != 1.0 || (float)(touches < 3) != 0.0f) return 3;
    enum ScanDirection direction = FORWARD;
    enum ScanDirection flipped = -direction;
    enum ScanDirection positive = +direction;
    flipped = -positive;
    if (scan(-direction) != -1 || scan(+flipped) != -1 || reverse(flipped) != FORWARD) return 4;
    if ((bool)-touches != true || (bool)+touches != true) return 5;
    printf("scalars %.1f %.1f %d %d\n", yes, (double)no, (int)flipped, touches);
    return 0;
}
