#include <stdio.h>
#include <atomic>
#include <thread>
extern "C" unsigned long long duckdb_rollback_allocation(int, int);
extern "C" int duckdb_rollback_guard(int);
extern "C" int duckdb_rollback_guard_attempts();
extern "C" int duckdb_rollback_guard_concurrent();
extern "C" int duckdb_rollback_guard_concurrent_attempts();
int main(int argc, char **) {
    if (argc > 1) {
        constexpr int count = 32;
        std::atomic<int> ready{0};
        std::atomic<bool> start{false};
        std::thread threads[count];
        int results[count] = {};
        for (int i = 0; i != count; ++i) {
            threads[i] = std::thread([&, i] {
                ready.fetch_add(1);
                while (!start.load()) std::this_thread::yield();
                results[i] = duckdb_rollback_guard_concurrent();
            });
        }
        while (ready.load() != count) std::this_thread::yield();
        start.store(true);
        for (auto &thread : threads) thread.join();
        int success = 0, retried = 0, bad = 0;
        for (int result : results) {
            if (result == 42) ++success;
            else if (result == -8) ++retried;
            else ++bad;
        }
        printf("concurrent %d %d %d %d\n", success, retried, bad, duckdb_rollback_guard_concurrent_attempts());
        return success == 31 && retried == 1 && bad == 0 && duckdb_rollback_guard_concurrent_attempts() == 2 ? 0 : 1;
    }
    for (int scenario = 0; scenario != 3; ++scenario)
        for (int fail = 0; fail != 4; ++fail)
            printf("allocation %d %d %llu\n", scenario, fail, duckdb_rollback_allocation(scenario, fail));
    int first = duckdb_rollback_guard(1);
    int retry = duckdb_rollback_guard(0);
    int published = duckdb_rollback_guard(1);
    printf("guard %d %d %d %d\n", first, retry, published, duckdb_rollback_guard_attempts());
    return first == -8 && retry == 42 && published == 42 && duckdb_rollback_guard_attempts() == 2 ? 0 : 1;
}
