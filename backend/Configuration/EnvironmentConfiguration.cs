public static class EnvironmentConfiguration
{
    public static WebApplicationBuilder LoadEnvironment(this WebApplicationBuilder builder)
    {
        DotNetEnv.Env.Load(FindEnvFile());

        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            $"Host=localhost;Port={Env("POSTGRES_HOST_PORT")};Database={Env("POSTGRES_DB")};Username={Env("POSTGRES_USER")};Password={Env("POSTGRES_PASSWORD")}";

        builder.Configuration["Jwt:Key"] = Env("JWT_KEY");
        builder.Configuration["Anthropic:ApiKey"] = Env("ANTHROPIC_API_KEY");
        builder.Configuration["Anthropic:BaseUrl"] = Env("ANTHROPIC_BASE_URL");

        // Optional: AI suggestions via Anthropic API
        var anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (anthropicKey is not null)
            builder.Configuration["Anthropic:ApiKey"] = anthropicKey;

        return builder;
    }

    private static string FindEnvFile()
    {
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
        var fullPath = Path.GetFullPath(envPath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException(
                $".env file not found. Looked in: {fullPath}");

        return fullPath;
    }

    private static string Env(string key) =>
        Environment.GetEnvironmentVariable(key) ??
        throw new InvalidOperationException($"Missing env var: {key}");
}