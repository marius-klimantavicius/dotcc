//type:fn
//options_all:--c++17 -w

struct Noncopyable {
  Noncopyable();
  Noncopyable(const Noncopyable &) = delete;
  virtual ~Noncopyable();
};
struct Derived : Noncopyable {};
struct NoncopyableAggr {
  Noncopyable nc;
};

Noncopyable make(int kind = 0) {
  switch (kind) {
  case 0: return {};
  case 1: return Noncopyable();
  case 2: return Noncopyable{};
  default: return make();
  }

}

template<typename T> struct Convert { operator T(); };

Noncopyable nrvo() {
  Noncopyable nrvo;
  return nrvo; // expected-error {{deleted constructor}}
}

Noncopyable nc1 = make();
Noncopyable nc3 = Derived(); // expected-error {{deleted constructor}}

Noncopyable ref_conv1 = Convert<Noncopyable&>(); // expected-error {{deleted constructor}}
Noncopyable ref_conv2((Convert<Noncopyable&>())); // expected-error {{deleted constructor}}
Noncopyable ref_conv3 = {Convert<Noncopyable&>()}; // expected-error {{deleted constructor}}
Noncopyable ref_conv4{Convert<Noncopyable&>()}; // expected-error {{deleted constructor}}

Noncopyable derived_conv1 = Convert<Derived>(); // expected-error {{deleted constructor}}
Noncopyable derived_conv2((Convert<Derived>())); // expected-error {{deleted constructor}}
Noncopyable derived_conv3 = {Convert<Derived>()}; // expected-error {{deleted constructor}}
Noncopyable derived_conv4{Convert<Derived>()}; // expected-error {{deleted constructor}}

NoncopyableAggr nc_aggr3 = {Convert<NoncopyableAggr>()}; // expected-error {{no viable conversion}}
NoncopyableAggr nc_aggr4{Convert<NoncopyableAggr>()}; // expected-error {{no viable conversion}}
NoncopyableAggr nc_aggr5 = Convert<Noncopyable>(); // expected-error {{no viable conversion}}
NoncopyableAggr nc_aggr6((Convert<Noncopyable>())); // expected-error {{no matching constructor}}

void test_expressions(bool b) {
  Noncopyable sc2 = static_cast<Noncopyable>(nc1); // expected-error {{deleted}}
  Noncopyable sc3 = static_cast<Noncopyable&&>(Noncopyable()); // expected-error {{deleted}}
  Noncopyable sc4 = static_cast<Noncopyable>(static_cast<Noncopyable&&>(Noncopyable())); // expected-error {{deleted}}

  Noncopyable cc2 = (Noncopyable)Derived(); // expected-error {{deleted}}

  Noncopyable fc2 = Noncopyable(Derived()); // expected-error {{deleted}}
}
