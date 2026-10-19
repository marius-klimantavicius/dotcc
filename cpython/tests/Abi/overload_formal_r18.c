#include <stdio.h>

#ifdef DOTCC_MANAGED_CONTROL
extern unsigned long choose(unsigned long value);
#else
unsigned long choose(unsigned long value) { return value + 1000UL; }
#endif

int main(void)
{
    int value = 7;
    unsigned long literal = choose(7);
    unsigned long effect = choose(value++);
    unsigned long variable = choose((unsigned long)value);
    printf("%lu %lu %lu %d\n", literal, effect, variable, value);
    return literal == 1007UL && effect == 1007UL && variable == 1008UL && value == 8 ? 0 : 25;
}
