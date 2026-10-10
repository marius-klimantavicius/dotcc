struct LocalTime { long seconds; long nanos; };
long local_first() {
  LocalTime ts;
  using Seconds=decltype(ts.seconds);
  Seconds result=3;
  {
    LocalTime ts;
    using NestedSeconds=decltype(ts.seconds);
    NestedSeconds nested=4;
    result+=nested;
  }
  return result;
}
long local_second() {
  LocalTime ts;
  using Seconds=decltype(ts.seconds);
  int seed=5;
  using Seed=decltype(seed+1);
  Seed sum=seed;
  Seconds result=sum;
  return result;
}
int main() { return local_first()==7 && local_second()==5?0:1; }
