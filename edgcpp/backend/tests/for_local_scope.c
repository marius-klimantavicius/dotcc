/* Adjacent declarations must not acquire the following for-loop's scope. */
static int separate_declaration(void) {
    int i = 0;
    for (; i < 4; ++i) { }
    return i;
}
static int multiple_declarators(void) {
    char text[] = "abcd";
    char *left = text, *right = left;
    for (; *left; left++, right++) { }
    return (int)(right - text);
}
static int original_for_declaration(void) {
    int sum = 0;
    for (int i = 0; i < 4; ++i) sum += i;
    int i = 3;
    return sum + i;
}
#define DECLARE_AND_LOOP int macro_index = 0; for (; macro_index < 2; ++macro_index) { }
static int macro_declaration(void) {
    DECLARE_AND_LOOP
    return macro_index;
}
int main(void) {
    return separate_declaration() == 4 && multiple_declarators() == 4 &&
           original_for_declaration() == 9 && macro_declaration() == 2 ? 0 : 1;
}
