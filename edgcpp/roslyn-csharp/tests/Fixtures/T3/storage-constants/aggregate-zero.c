/* Genuine C constant-record value candidate matching the retained 24/8 shape. */
struct Item { int *first; const char *second; int third; int fourth; };
static const struct Item zero_item = {0};
void reset_item(struct Item *value) { *value = zero_item; }
int fixture(void) {
    int anchor = 7;
    struct Item value = {&anchor, "seed", 23, 29};
    reset_item(&value);
    return (value.first == 0) + (value.second == 0) + (value.third == 0) + (value.fourth == 0);
}
