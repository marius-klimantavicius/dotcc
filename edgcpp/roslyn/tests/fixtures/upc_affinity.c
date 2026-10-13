int upc_affinity_calls;
int upc_affinity_effect(int index) {
  ++upc_affinity_calls;
  return index % THREADS;
}
int upc_affinity_body(int index) { return index + 1; }
int upc_affinity_loop(void) {
  int total = 0;
  upc_forall(int index = 0; index < 6; ++index; upc_affinity_effect(index))
    total += upc_affinity_body(index);
  return total;
}
