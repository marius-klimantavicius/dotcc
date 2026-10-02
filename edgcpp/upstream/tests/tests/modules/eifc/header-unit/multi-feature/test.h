using td_char          = char;
using td_int           = int;
using td_unsigned_int  = unsigned int;

namespace my_namespace {
  struct my_class {
    my_class();
    ~my_class();

    int do_stuff(td_char, td_int);
    static long do_stuff(td_unsigned_int, td_int);
  };

  template<typename a_Value_type>
  struct my_optional {
    my_optional() : storing_value(false)
      {}
    my_optional(const a_Value_type &value)
      : storing_value(true), stored_value(value)
      {}
    my_optional(a_Value_type &&value)
      : storing_value(true), stored_value(static_cast<a_Value_type &&>(value))
      {}
    my_optional(const my_optional<a_Value_type> &other)
      : storing_value(other.storing_value)
      {
        if (storing_value) {
          ::new (&stored_value) a_Value_type(other.stored_value);
        }  /* if */
      }

    my_optional(my_optional<a_Value_type> &&other)
      : storing_value(other.storing_value)
      {
        if (storing_value) {
          ::new (&stored_value) a_Value_type(
                                 static_cast<a_Value_type &&>(other.stored_value));
        }  /* if */
      }
    ~my_optional() {
      if (storing_value) {
        stored_value.~a_Value_type();
      }  /* if */
    }
    bool has_value() const {
      return storing_value;
    }
    /* Value retrieval functions. */
    a_Value_type* operator->() {
      return &stored_value;
    }
    const a_Value_type* operator->() const {
      return &stored_value;
    }
    a_Value_type& operator*() {
      return stored_value;
    }
    const a_Value_type& operator*() const {
      return stored_value;
    }
    /* Value update functions. */
    my_optional<a_Value_type>& operator=(const a_Value_type &value) {
      if (storing_value) {
        stored_value = value;
      } else {
        storing_value = true;
        ::new (&stored_value) a_Value_type(value);
      }  /* if */
      return *this;
    }
    my_optional<a_Value_type>& operator=(a_Value_type &&value) {
      if (storing_value) {
        stored_value = static_cast<a_Value_type &&>(value);
      } else {
        storing_value = true;
        ::new (&stored_value) a_Value_type(static_cast<a_Value_type &&>(value));
      }  /* if */
      return *this;
    }
    my_optional<a_Value_type>& operator=(const my_optional<a_Value_type> &other) {
      if (other.storing_value) {
        *this = other.stored_value;
      } else {
        this->clear();
      }  /* if */
      return *this;
    }
    my_optional<a_Value_type>& operator=(my_optional<a_Value_type> &&other) {
      if (other.storing_value) {
        *this = static_cast<a_Value_type &&>(other.stored_value);
      } else {
        this->clear();
      }  /* if */
      return *this;
    }
    void clear() {
      if (storing_value) {
        stored_value.~a_Value_type();
      }  /* if */
      storing_value = false;
    }
  private:
    bool storing_value;
    union {
      a_Value_type stored_value;
    };
  };

  struct point {
    int x;
    int y;
    int z;
  };

  template<typename T>
  T add_points(T a, T b) {
    T value = {a.x + b.x, a.y + b.y, a.z + b.z};
    return value;
  }
}
