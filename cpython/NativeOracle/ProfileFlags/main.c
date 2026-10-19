#include <stdio.h>
int public_role(void);
int limited_role(void);
int internal_role(void);
int main(void) {
    printf("%d %d %d\n", public_role(), limited_role(), internal_role());
    return 0;
}
