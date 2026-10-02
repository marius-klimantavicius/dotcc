//options_all:-r -x -tused
//options: --strict;cp

enum e { a = 0, b = 255 };  // Fits in unsigned char
struct S { enum e : 16; };  // Okay even though the bit field is
                            // bigger than the underlying enum type;
                            // the programmer doesn't know what size
                            // was chosen for the enum.

struct SS { enum e : 33; };
struct SSS { enum e : 65; };

