//options_all:-r -x -tused
//options: --strict;cp

// Bug EDGqa00600
// [jsa: this bug was introduced in 2.34.  The example works if done without
// typedefs.]

typedef char              MSG[];

MSG bar = "This is bar";
const MSG foo = "This is longer than bar";

typedef const MSG CMSG;
extern CMSG x = "Another string";

