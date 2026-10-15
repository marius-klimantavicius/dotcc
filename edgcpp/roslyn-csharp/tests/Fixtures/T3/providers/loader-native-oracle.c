/* Independent native reference process only; never a generated dependency. */
#include <stdio.h>
extern const char *loader_provider_path;
extern int fixture(void);
extern int loader_observation_count(void);
extern int loader_observe(int);
int main(int count, char **arguments) {
    if (count != 2) return 2;
    loader_provider_path = arguments[1];
    printf("result=%d\n", fixture());
    for (int index = 0; index < loader_observation_count(); ++index)
        printf("%d:%d\n", index, loader_observe(index));
    return 0;
}
