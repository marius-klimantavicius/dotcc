extern "C" void transfer_point(int);
static int next_transfer_id;
struct TransferGuard {
    int id;
    TransferGuard():id(++next_transfer_id) {}
    TransferGuard(int value):id(value) {}
    ~TransferGuard() noexcept(false) { transfer_point(id); }
};
extern "C" void array_runtime_transfer() {
    next_transfer_id = 0;
    TransferGuard before(4);
    TransferGuard array[3];
    TransferGuard after(5);
    transfer_point(99);
}
