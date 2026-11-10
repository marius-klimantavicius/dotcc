// Reduced gallery label regression, plus C general-format precision and
// rounded exponent boundaries. The gcc oracle checks this same program.
#include <stdio.h>

int main(void)
{
    float used = 18.4f;
    char label[64];
    int count = snprintf(label, sizeof(label), "%g / %g GB", used, 20.0f);
    printf("%d: %s\n", count, label);
    printf("default: %g %G\n", used, used);
    printf("precision: %.0g %.0G %.g %.1g %.3g %.8G %.17g\n",
           used, used, 1.25, 1.25, 12.5, 123.456789, used);
    printf("upper: %g %g %G %g %.3g %.3G\n",
           999999.0, 1000000.0, 1000000.0, 999999.5, 999.5, 999.5);
    printf("lower: %g %g %G %g\n", 0.0001, 0.00001, 0.00001, 0.00009999996);
    printf("trim/sign: %.6g %g %g %g\n", 1.25, 0.0, -0.0, -1234567.0);
    printf("dynamic: %.*g %.*g %.*g\n",
           -1, 3.141592653589793, 0, 3.141592653589793, 4, 3.141592653589793);
    return 0;
}
