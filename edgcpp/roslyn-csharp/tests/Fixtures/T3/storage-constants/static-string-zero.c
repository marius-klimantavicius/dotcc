/* Genuine direct static string and initk_none record storage activation. */
struct StaticState { int *link; long value; int count; };
char *static_text(void) {
    static char text[] = "A\0BC";
    return text;
}
struct StaticState *static_zero(void) {
    static struct StaticState state;
    return &state;
}
int fixture(void) {
    char *text = static_text();
    struct StaticState *state = static_zero();
    state->value += 7;
    ++state->count;
    ++text[2];
    return (state->link == 0) + text[0] + text[1] + text[2] + text[3] + text[4]
        + (int)state->value + state->count;
}
