struct Interface { virtual int value() = 0; };
int dispatch(Interface *object) { return object->value(); }
