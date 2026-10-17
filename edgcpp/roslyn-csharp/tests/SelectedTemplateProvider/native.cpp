#include "../Fixtures/T5/selected-template-provider.hpp"
#include <cstdio>

int main() {
    for (int run = 0; run != 2; ++run) {
        const int result = fixture();
        const int value_count = lifetime_value_count();
        const int event_count = lifetime_count();
        std::printf("{\"result\":%d,\"values\":[", result);
        for (int index = 0; index != value_count; ++index) {
            if (index) std::printf(",");
            std::printf("%d", lifetime_value(index));
        }
        std::printf("],\"events\":[");
        for (int index = 0; index != event_count; ++index) {
            if (index) std::printf(",");
            std::printf("%d", lifetime_event(index));
        }
        std::printf("],\"count\":%d,\"overflow\":%d,\"live\":%d,\"constructed\":%d,\"destroyed\":%d}\n",
                    event_count, lifetime_overflow(), lifetime_live(),
                    lifetime_constructed(), lifetime_destroyed());
        if (value_count != 9 || event_count < 0 || event_count > 64 ||
            lifetime_overflow() != 0 || lifetime_live() != 0 ||
            lifetime_constructed() != lifetime_destroyed()) return 1;
    }
}
