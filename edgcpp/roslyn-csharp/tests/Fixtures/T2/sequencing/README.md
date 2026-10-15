# Sequencing candidates

These sources are genuine separate single-TU export inputs. Public-graph
inspection and independent native observations are complete. Generated execution
remains pending; no sequencing acceptance is claimed by these input checks.

| Input | Native result | Ordered events | Value observations |
|---|---:|---|---|
| `branches.c` | 41 | 1,2,3,4,5,6,7,8,9,10,11,12,13,20,30,21,31,22,32,23,40 | 21 |
| `places.cpp` | 15 | 2,1,5,6,6,6,7,6,6,6,9,8,11,10 | 23,-14,-13,-13,15,0,0,6,22,-12,1 |
| `callee.cpp` | 18 | 1,2,3,1,2,3 | 6 |
| `aggregate-snapshot.cpp` | 248 | 1,2,1,2 | 7,11,13,17,99,101 |

The native oracle prints all events and requested value observations. Link it as
C with the C application, or compile it as C and link that object with each C++
application. C++ sources require C++20; the C source requires C17. No application
body or oracle object belongs in the generated workload's dependency closure.

`places.cpp` intentionally exercises signed bitfield truncation under the pinned
GNU target, plus unsigned wraparound. The expected values must be confirmed by
the independent native oracle and retained field facts before acceptance.

`aggregate-snapshot.cpp` materializes explicit named-local copies before the
destination selector mutates source. Initialization and assignment must copy
bytes at their retained occurrences.

The original `aggregate.cpp` remains an unresolved comparison case, excluded from
generated acceptance. Its native GCC result was600, with values99,101 repeated
three times. Its retained EDG roots instead contain explicit comma sequences
assigning source into a selected local before calling the destination selector;
those roots predict248 and values7,11,13,17,99,101. The initial source-based
prediction430 was incorrect. Original source, object, native receipt and public
dump remain preserved in the external sequencing input artifact. This consumer
does not change the retained schedule to reproduce GCC's result.

`callee.cpp` has one argument to avoid imposing an independent compiler's choice
of order among arguments. Inspect actual raw operand flags and any explicit
lowered temporary/comma occurrences; no source-language reorder pass is allowed.
