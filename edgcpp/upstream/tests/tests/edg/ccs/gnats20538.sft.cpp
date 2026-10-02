//type:fp
//options:-A
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

void take(Noncopyable nc) {}

Noncopyable nc1 = make();
Noncopyable nc2 = Noncopyable();
Noncopyable nc4((Noncopyable()));
Noncopyable nc5 = {Noncopyable()};
Noncopyable nc6{Noncopyable()};

NoncopyableAggr nca1 = NoncopyableAggr{};
NoncopyableAggr nca2 = NoncopyableAggr{{}};
NoncopyableAggr nca3 = NoncopyableAggr{NoncopyableAggr{Noncopyable()}};

template<typename T> struct Convert { operator T(); };
Noncopyable conv1 = Convert<Noncopyable>();
Noncopyable conv2((Convert<Noncopyable>()));
Noncopyable conv3 = {Convert<Noncopyable>()};
Noncopyable conv4{Convert<Noncopyable>()};

NoncopyableAggr nc_aggr1 = Convert<NoncopyableAggr>();
NoncopyableAggr nc_aggr2((Convert<NoncopyableAggr>()));
NoncopyableAggr nc_aggr7 = {Convert<Noncopyable>()};
NoncopyableAggr nc_aggr8{Convert<Noncopyable>()};

void test_expressions(bool b) {
  auto lambda = [a = make()] {};

  take({});
  take(Noncopyable());
  take(Noncopyable{});
  take(make());

  Noncopyable &&dc1 = dynamic_cast<Noncopyable&&>(Noncopyable());
  Noncopyable &&dc2 = dynamic_cast<Noncopyable&&>(nc1);
  Noncopyable &&dc3 = dynamic_cast<Noncopyable&&>(Derived());

  Noncopyable sc1 = static_cast<Noncopyable>(Noncopyable());

  Noncopyable cc1 = (Noncopyable)Noncopyable();

  Noncopyable fc1 = Noncopyable(Noncopyable());

  extern const volatile Noncopyable make_cv();
  Noncopyable cv_difference1 = make_cv();
  const volatile Noncopyable cv_difference2 = make();
}
