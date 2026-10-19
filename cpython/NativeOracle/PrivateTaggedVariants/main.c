#include <stdio.h>
int first_size(void);
int first_value(void);
int second_size(void);
int second_value(void);
int main(void) {
    printf("%d %d %d %d\n", first_size(), second_size(), first_value(), second_value());
    return first_size()==16 && second_size()==24 && first_value()==7 && second_value()==42 ? 0 : 1;
}
