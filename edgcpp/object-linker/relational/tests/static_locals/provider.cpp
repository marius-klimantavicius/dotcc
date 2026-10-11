#include "program.h"
int *early_address = &forward_value;
int *remote_address = &remote_value;
Links early_links{&forward_value, &remote_value};
int *early_addresses[2] = {&forward_value, &remote_value};
int forward_value = 17;
int ordinary = 40;
double ratio = 1.5;
bool enabled = true;
unsigned __int128 wide = ((unsigned __int128)1 << 100) + 7;
Choice choice = Choice::first;
int bump_shared() { return ++shared_counter(); }
int *provider_shared() { return &shared_counter(); }
char *provider_seed() { return shared_seed(); }
int **provider_pointer() { return shared_pointer_address(); }
int **provider_forward() { return shared_forward_address(); }
double *provider_ratio() { return &shared_ratio(); }
int *provider_local() { return &local_counter(); }
int *scoped(bool first) {
    if (first) { static int value = 21; return &value; }
    else { static int value = 31; return &value; }
}
int *global_address() { return &ordinary; }
const Pair *provider_pair() { return &shared_pair(); }
int *provider_array() { return shared_array(); }
