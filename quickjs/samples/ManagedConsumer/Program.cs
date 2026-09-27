using Managed.Interpreters;

const string expected = "{\"values\":[4,6,10],\"sum\":20}";
for (int iteration = 0; iteration < 2; iteration++)
{
    var runtime = new QuickJsRuntime(new Dictionary<string, string>
    {
        ["sum"] = "export const sum = values => values.reduce((a, b) => a + b, 0);"
    });
    using (runtime)
    using (var context = runtime.CreateContext())
    {
        int callbacks = 0;
        context.RegisterFunction("hostDouble", arguments => { callbacks++; return arguments[0] * 2; });
        using var input = context.CreateString("{\"items\":[2,3,5]}");
        context.SetGlobal("inputJson", input);
        using var evaluation = context.Evaluate("""
            import { sum } from 'sum';
            const input = JSON.parse(inputJson);
            Promise.resolve(input.items.map(x => hostDouble(x))).then(values => {
                globalThis.result = JSON.stringify({values, sum: sum(values)});
            });
            """, "consumer.mjs", module: true);
        if (runtime.DrainJobs() < 1) throw new Exception("Promise continuation was not executed.");
        using var result = context.GetGlobal("result");
        if (result.ToString() != expected || callbacks != 3 || runtime.ModuleLoads != 1)
            throw new Exception($"Consumer mismatch: {result}, callbacks={callbacks}, modules={runtime.ModuleLoads}.");
        Console.WriteLine(result.ToString());
    }
    if (runtime.AllocatedBlocks != 0 || runtime.AllocatedBytes != 0) throw new Exception("Runtime did not release all allocations.");
}
