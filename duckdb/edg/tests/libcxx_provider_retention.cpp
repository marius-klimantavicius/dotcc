// Fixture-only ownership anchors for the separate-consumer proof.
// These are not part of the general DuckDB provider or a full-library solution.
#include <memory>
#include <type_traits>

const unsigned long* dotgen_force_integral_value =
    &std::integral_constant<unsigned long, 2UL>::value;
template class std::__compressed_pair<int*, std::allocator<int>>;
