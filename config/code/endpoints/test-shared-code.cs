[endpoint: Curiosity.Endpoints.Path("test-shared-code")]
[endpoint: Curiosity.Endpoints.SyncWithRequest]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

using SharedCode.Greeter;
using SharedCode.MathUtils;
using SharedCode.GraphHelpers;
using SharedCode.DerivedValues;

// Body is the raw request body; treat it as the name to greet (falls back to "World").
var who = string.IsNullOrWhiteSpace(Body) ? "World" : Body.Trim();

return new
{
    greeting = SharedCode.Greeter.Greeter1.Greeter.Hello(who),
    sum = MathUtils.Add(40, 2),
    fib10 = MathUtils.Fibonacci(10),   // 55
    derivedAnswer = DerivedValues.Answer,      // 42 (Samples.Base + 1)
    reviewNodeType = N.Device.Type,             // schema helper, no using
    reviewCount = GraphHelpers.CountDevices(Graph), // shared code given the endpoint's Graph
    calledBy = CurrentUser
};

