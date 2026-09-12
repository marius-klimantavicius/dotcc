/* Constants from stream receive completion and internal send flags. */
typedef enum Flags { NONE = 0, ONE = 1 } Flags;
long completion_mask(void) { return (long)0x8000000000000000ULL; }
Flags set_flags(Flags flags) { flags |= (Flags)0x80000000U; return flags; }
