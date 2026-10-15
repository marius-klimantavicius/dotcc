/* Retained RHS computation types and selected conditional record values. */
struct Value { long long number; };
struct Token { long long a; int b; };
static struct Value value;
static struct Token left = {17, 19};
static struct Token right = {29, 31};
static struct Token destination;
static long long observations[22];
static int place_calls, rhs_calls, left_calls, right_calls, destination_calls;
static struct Value *selected_value(void) { ++place_calls; return &value; }
static unsigned long selected_rhs(void) { ++rhs_calls; return 4UL; }
static struct Token *selected_token(struct Token *token) {
    if (token == &left) ++left_calls; else ++right_calls;
    return token;
}
static struct Token *selected_destination(void) { ++destination_calls; return &destination; }
static void copy_selected(int selected) {
    *selected_destination() = selected ? *selected_token(&left) : *selected_token(&right);
}
int fixture(void) {
    place_calls = 0; rhs_calls = 0; left_calls = 0; right_calls = 0; destination_calls = 0;
    long long signed64 = 1;
    unsigned long high = 0x8000000000000000UL;
    signed64 += high; observations[0] = signed64;
    signed64 -= high; observations[1] = signed64;
    signed64 = 3; signed64 *= 5UL; observations[2] = signed64;
    int signed32 = 4;
    signed32 += 7UL; observations[3] = signed32;
    signed32 = -3; signed32 += 4294967301LL; observations[4] = signed32;
    unsigned int unsigned32 = 3;
    unsigned32 += -5LL; observations[5] = unsigned32;
    unsigned32 = 9; unsigned32 += 4294967303UL; observations[6] = unsigned32;
    signed32 = -3; signed32 += 5U; observations[7] = signed32;
    signed32 = 256; signed32 |= 0x80000000U; observations[8] = signed32;
    unsigned char narrow = 250;
    narrow += 10U; observations[9] = narrow;
    value.number = 5;
    long long result = (selected_value()->number += selected_rhs());
    observations[10] = result;
    observations[11] = value.number;
    observations[12] = place_calls;
    observations[13] = rhs_calls;
    copy_selected(1); observations[14] = destination.a + destination.b;
    copy_selected(0); observations[15] = destination.a + destination.b;
    observations[16] = left_calls;
    observations[17] = right_calls;
    observations[18] = destination_calls;
    observations[19] = sizeof(struct Token);
    short unchanged_helper = 9;
    unchanged_helper -= 3; observations[20] = unchanged_helper;
    unsigned char unchanged_bits = 15;
    unchanged_bits &= 6; observations[21] = unchanged_bits;
    return 22;
}
int compound_count(void) { return 22; }
long long compound_observe(int index) { return observations[index]; }
