//options_all:-r -x -tused
//options: --strict;cp

class exception {
public:
virtual ~exception() throw();
};
class logic_error : public exception {
//  ~logic_error() throw();
};


