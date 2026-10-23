using System.Runtime.InteropServices;
using System.Text.Json;
using Raw = Managed.Security.S2n.S2nTls;
unsafe
{
    if (args.Length != 2) throw new ArgumentException("native observations and output report required");
    var expected = File.ReadAllLines(args[0]); var values = new List<string>();
    void Layout<T>(string name) where T : unmanaged
    {
        Alignment<T> aligned = default;
        values.Add($"{name}:{sizeof(T)}:{(byte*)&aligned.value-(byte*)&aligned}");
    }
    void Field(string name, void* member, void* owner) => values.Add($"{name}:{(byte*)member-(byte*)owner}");
    void Bytes(string name, void* owner, int length) => values.Add(name + ":" + Convert.ToHexString(new ReadOnlySpan<byte>(owner,length)).ToLowerInvariant());
    Raw.s2n_blob s2n_blob = default;
    Layout<Raw.s2n_blob>("s2n_blob");
    Field("s2n_blob.data", &s2n_blob.data, &s2n_blob);
    Field("s2n_blob.size", &s2n_blob.size, &s2n_blob);
    Field("s2n_blob.allocated", &s2n_blob.allocated, &s2n_blob);
    Raw.s2n_stuffer s2n_stuffer = default;
    Layout<Raw.s2n_stuffer>("s2n_stuffer");
    Field("s2n_stuffer.blob", &s2n_stuffer.blob, &s2n_stuffer);
    Field("s2n_stuffer.read_cursor", &s2n_stuffer.read_cursor, &s2n_stuffer);
    Field("s2n_stuffer.write_cursor", &s2n_stuffer.write_cursor, &s2n_stuffer);
    Field("s2n_stuffer.high_water_mark", &s2n_stuffer.high_water_mark, &s2n_stuffer);
    Raw.s2n_hash_state s2n_hash_state = default;
    Layout<Raw.s2n_hash_state>("s2n_hash_state");
    Field("s2n_hash_state.hash_impl", &s2n_hash_state.hash_impl, &s2n_hash_state);
    Field("s2n_hash_state.alg", &s2n_hash_state.alg, &s2n_hash_state);
    Field("s2n_hash_state.digest", &s2n_hash_state.digest, &s2n_hash_state);
    Raw.s2n_cipher_suite s2n_cipher_suite = default;
    Layout<Raw.s2n_cipher_suite>("s2n_cipher_suite");
    Field("s2n_cipher_suite.name", &s2n_cipher_suite.name, &s2n_cipher_suite);
    Field("s2n_cipher_suite.iana_value", s2n_cipher_suite.iana_value, &s2n_cipher_suite);
    Field("s2n_cipher_suite.key_exchange_alg", &s2n_cipher_suite.key_exchange_alg, &s2n_cipher_suite);
    Field("s2n_cipher_suite.record_alg", &s2n_cipher_suite.record_alg, &s2n_cipher_suite);
    Field("s2n_cipher_suite.sslv3_record_alg", &s2n_cipher_suite.sslv3_record_alg, &s2n_cipher_suite);
    Field("s2n_cipher_suite.minimum_required_tls_version", &s2n_cipher_suite.minimum_required_tls_version, &s2n_cipher_suite);
    Raw.s2n_security_policy s2n_security_policy = default;
    Layout<Raw.s2n_security_policy>("s2n_security_policy");
    Field("s2n_security_policy.cipher_preferences", &s2n_security_policy.cipher_preferences, &s2n_security_policy);
    Field("s2n_security_policy.kem_preferences", &s2n_security_policy.kem_preferences, &s2n_security_policy);
    Field("s2n_security_policy.rules", &s2n_security_policy.rules, &s2n_security_policy);
    Raw.s2n_config s2n_config = default;
    Layout<Raw.s2n_config>("s2n_config");
    Field("s2n_config.security_policy", &s2n_config.security_policy, &s2n_config);
    Raw.s2n_connection s2n_connection = default;
    Layout<Raw.s2n_connection>("s2n_connection");
    Field("s2n_connection.config", &s2n_connection.config, &s2n_connection);
    Field("s2n_connection.security_policy_override", &s2n_connection.security_policy_override, &s2n_connection);
    Field("s2n_connection.context", &s2n_connection.context, &s2n_connection);
    Field("s2n_connection.send", &s2n_connection.send, &s2n_connection);
    Field("s2n_connection.recv", &s2n_connection.recv, &s2n_connection);
    Field("s2n_connection.client_protocol_version", &s2n_connection.client_protocol_version, &s2n_connection);
    Field("s2n_connection.actual_protocol_version", &s2n_connection.actual_protocol_version, &s2n_connection);
    s2n_blob.growable = 1; Bytes("blob-bitfields", &s2n_blob, sizeof(Raw.s2n_blob));
    s2n_stuffer.alloced = 1; s2n_stuffer.tainted = 1; Bytes("stuffer-bitfields", &s2n_stuffer, sizeof(Raw.s2n_stuffer));
    s2n_config.use_tickets = 1; s2n_config.disable_x509_validation = 1; Bytes("config-bitfields", &s2n_config, 8);

    if (!values.SequenceEqual(expected)) throw new InvalidOperationException("Canonical product/native ABI differs:\n" + string.Join("\n", values));
    delegate*<int*> callback = &Raw.s2n_errno_location;
    if (callback != (delegate*<int*>)&Raw.s2n_errno_location || callback() != Raw.s2n_errno_location()) throw new Exception("Canonical function address mismatch");
    using var barrier = new Barrier(2);
    Exception? failure = null;
    var threads = Enumerable.Range(1, 2).Select(value => new Thread(() =>
    {
        try
        {
            int* first = Raw.s2n_errno_location(); *first = value;
            barrier.SignalAndWait();
            if (*first != value || first != Raw.s2n_errno_location()) throw new Exception("Canonical TLS errno identity/value leaked between threads");
        }
        catch (Exception error) { Interlocked.CompareExchange(ref failure, error, null); }
    })).ToArray();
    foreach (var thread in threads) thread.Start();
    foreach (var thread in threads) thread.Join();
    if (failure is not null) throw failure;
    using var output = File.Create(args[1]); using var json = new Utf8JsonWriter(output);
    json.WriteStartObject(); json.WriteString("status", "passed"); json.WriteString("scope", "canonical linked product/native layouts, offsets, bitfields, managed callback address and thread-local error identity");
    json.WriteStartArray("observations"); foreach (var value in values) json.WriteStringValue(value); json.WriteEndArray(); json.WriteEndObject();
    Console.WriteLine("PASS canonical product/native ABI, callback and thread-local errors");
}
[StructLayout(LayoutKind.Sequential)]
struct Alignment<T> where T : unmanaged { public byte prefix; public T value; }
