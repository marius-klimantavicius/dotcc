//options_all:-r -x -tused
//options: --strict;cn:;cp

namespace N {
	static union {
		int i;
	};
  int j = i;
};
int j = N::i;
using namespace N;
int k = i;


