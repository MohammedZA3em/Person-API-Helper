using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

public class FullLogActionFilter : IAsyncActionFilter
{
    // قائمة الحقول الحساسة التي سيتم حجبها
    private static readonly string[] SensitiveFields = { "password", "token", "creditCard" };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // ===============================
        // 1️⃣ جمع معلومات عن الطلب
        // ===============================
        var controller = context.Controller.GetType().Name;
        var action = context.ActionDescriptor.DisplayName;
        var httpMethod = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;
        var queryString = context.HttpContext.Request.QueryString.ToString();

        // ===============================
        // 2️⃣ قراءة Body إذا كان POST أو PUT (أقل من 10KB فقط)
        // ===============================
        string bodyAsText = string.Empty;
        if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put)
        {
            context.HttpContext.Request.EnableBuffering();
            using var reader = new StreamReader(
                context.HttpContext.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true
            );

            var bodyLength = context.HttpContext.Request.ContentLength ?? 0;
            if (bodyLength < 10 * 1024) // 10 KB
            {
                bodyAsText = await reader.ReadToEndAsync();
                bodyAsText = MaskSensitiveData(bodyAsText); // حماية البيانات الحساسة
                context.HttpContext.Request.Body.Position = 0;
            }
        }

        // ===============================
        // 3️⃣ جمع Parameters وحجب البيانات الحساسة
        // ===============================
        var parameters = JsonSerializer.Serialize(context.ActionArguments);
        parameters = MaskSensitiveData(parameters);

        var combinedInfo = string.IsNullOrWhiteSpace(bodyAsText)
            ? parameters
            : $"Parameters: {parameters}, Body: {bodyAsText}";

        // ===============================
        // 4️⃣ بدء Stopwatch لتسجيل زمن التنفيذ
        // ===============================
        var stopwatch = Stopwatch.StartNew();

        // ===============================
        // 5️⃣ تسجيل بداية التنفيذ
        // ===============================
        Log.Information(
            "Started {Controller}.{Action} | HTTP {Method} {Path}{Query} | {Info}",
            controller, action, httpMethod, path, queryString, combinedInfo
        );

        // ===============================
        // 6️⃣ تنفيذ الـ Action
        // ===============================
        var executedContext = await next();

        // ===============================
        // 7️⃣ تسجيل نهاية التنفيذ ووقت الاستجابة
        // ===============================
        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (executedContext.Exception != null)
        {
            Log.Error(
                executedContext.Exception,
                "Exception in {Controller}.{Action} | Elapsed: {Elapsed} ms",
                controller, action, elapsedMs
            );
        }
        else
        {
            Log.Information(
                "Finished {Controller}.{Action} | Elapsed: {Elapsed} ms",
                controller, action, elapsedMs
            );
        }
    }

    // ===============================
    // 8️⃣ دالة لحجب البيانات الحساسة
    // ===============================
    private string MaskSensitiveData(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var masked = MaskElement(root);
            return JsonSerializer.Serialize(masked);
        }
        catch
        {
            return json; // إذا لم يكن JSON صالحًا
        }
    }

    private object MaskElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in element.EnumerateObject())
            {
                if (SensitiveFields.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                    dict[prop.Name] = "***";
                else
                    dict[prop.Name] = MaskElement(prop.Value);
            }
            return dict;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray().Select(MaskElement).ToList();
        }
        else
        {
            return element.GetRawText();
        }
    }
}
