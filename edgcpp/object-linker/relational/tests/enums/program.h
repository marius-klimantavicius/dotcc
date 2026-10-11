enum class Status : unsigned short { idle = 2, ready = 513 };
enum Legacy { negative = -3, positive = 6 };
enum class Wide : unsigned long long { high = 18446744073709551615ULL };

struct Packet {
    Status status;
    Legacy code;
};

Status next_status(Status value);
int score(Packet *packet);
Wide wide_value();
