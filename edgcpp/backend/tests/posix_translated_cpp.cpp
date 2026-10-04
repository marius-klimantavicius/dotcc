#if defined(POSIX_NAMESPACE_COLLISION)
namespace ForeignNamespace { int getpid(); }
int main() { return ForeignNamespace::getpid(); }
#elif defined(POSIX_CLASS_COLLISION)
struct ForeignClass { static int getpid(); };
int main() { return ForeignClass::getpid(); }
#else
#if !defined(POSIX_BARE_CPP_HEADER)
extern "C" {
#endif
#include <unistd.h>
#if !defined(POSIX_BARE_CPP_HEADER)
}
#endif
int main() { return ::getpid() > 0 ? 0 : 1; }
#endif
