struct ThrowFirst { int first; };
struct ThrowHidden { int hidden; };
struct ThrowSecond { int second; };

struct ThrowObject : public ThrowFirst, private ThrowHidden, public ThrowSecond {
  void throw_member_object() { throw *this; }
  void throw_member_pointer() { throw this; }
};

void throw_public_object(const ThrowObject &object) { throw object; }
void throw_public_pointer(ThrowObject *object) { throw object; }
