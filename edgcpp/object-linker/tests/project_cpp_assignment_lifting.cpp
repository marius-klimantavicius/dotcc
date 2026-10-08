/* C++17 RHS-before-LHS sequencing must survive C# statement expansion. */
static int source_value;
static int destination;
static int touches;
static int reads;
static int read_source() { ++reads; return source_value; }
static int *touch_destination() {
    ++touches;
    source_value = 9;
    destination = 20;
    return &destination;
}

int main() {
    int x = 1;
    int result = (x += (x = 3));
    if (x != 6 || result != 6) return 1;
    x = 4;
    result = (x /= (x = 2));
    if (x != 1 || result != 1) return 2;
    short small = 1;
    result = (small += (small = 3));
    if (small != 6 || result != 6) return 3;

    source_value = 7;
    result = (*touch_destination() = read_source());
    if (touches != 1 || source_value != 9 || destination != 7 || result != 7) return 4;
    source_value = 7;
    result = (*touch_destination() += read_source());
    if (touches != 2 || reads != 2 || destination != 27 || result != 27) return 5;

    int false_value = 0;
    x = 11;
    result = false_value && (x += (x = 3));
    if (result || x != 11) return 6;
    result = !false_value || (x += (x = 3));
    if (!result || x != 11) return 7;
    result = false_value ? (x += (x = 3)) : 41;
    if (result != 41 || x != 11) return 8;
    result = !false_value ? (x += (x = 3)) : 41;
    if (result != 6 || x != 6) return 9;

    int count = 0;
    int body_count = 0;
    while ((x += (x = 1)) == 2 && count++ < 2) {
        ++body_count;
        continue;
    }
    if (count != 3 || body_count != 2 || x != 2) return 10;
    count = 0;
    do {
        ++count;
        continue;
    } while ((x += (x = 2)) == 4 && count < 2);
    if (count != 2 || x != 4) return 11;
    int step = 0;
    body_count = 0;
    for (int i = 0; (x += (x = 1)) == 2 && i < 3; ++i, step += (step = 3)) {
        if (i == 1) continue;
        ++body_count;
    }
    if (body_count != 2 || x != 2 || step != 6) return 12;
    return 0;
}
