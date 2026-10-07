/* C preserves this hardware register ABI; managed pinned storage cannot. */
register unsigned long hardware_value __asm__("r12");
int main() { hardware_value = 7; return hardware_value == 7 ? 0 : 1; }
