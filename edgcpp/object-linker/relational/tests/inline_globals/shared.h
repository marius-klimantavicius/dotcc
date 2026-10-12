inline constexpr char digits[] = {'0', '1', '2', '3'};
inline int dormant_shared = 51;

namespace left { inline int counter = 7; inline int dormant = 19; }
namespace right { inline int counter = 11; inline int dormant = 23; }

const char* provider_digits();
int provider_count();
extern int strong_value;
