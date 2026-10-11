inline int &shared_counter() { static int value = 4; return value; }
inline char *shared_seed() { static char value; return &value; }
inline int **shared_pointer_address() { static int *value = nullptr; return &value; }
extern int forward_value;
inline int **shared_forward_address() { static int *value = &forward_value; return &value; }
inline double &shared_ratio() { static double value = 2.5; return value; }
static int &local_counter() { static int value = 10; return value; }
extern int ordinary;
extern int *early_address;
extern int remote_value;
extern int *remote_address;
struct Links { int *first; int *second; };
extern Links early_links;
extern int *early_addresses[2];
extern double ratio;
extern bool enabled;
extern unsigned __int128 wide;
enum class Choice : unsigned char { first = 3, second = 9 };
extern Choice choice;
int bump_shared();
int *provider_shared();
char *provider_seed();
int **provider_pointer();
int **provider_forward();
double *provider_ratio();
int *provider_local();
int *scoped(bool first);
int *global_address();
struct Pair { int first; int second; };
inline const Pair &shared_pair() { static const Pair value{6, 8}; return value; }
inline int *shared_array() { static int value[3] = {2, 4}; return value; }
const Pair *provider_pair();
int *provider_array();
