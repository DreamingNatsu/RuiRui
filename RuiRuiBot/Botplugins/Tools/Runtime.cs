namespace RuiRuiBot.Botplugins.Tools {
    public static class Runtime {
        public static string Get()
#if NET11
=> ".Net Framework 1.1";
#elif NET20
=> ".Net Framework 2.0";
#elif NET35
=> ".Net Framework 3.5";
#elif NET40
=> ".Net Framework 4.0";
#elif NET45
=> ".Net Framework 4.5";
#elif NET451
=> ".Net Framework 4.5.1";
#elif NET452
=> ".Net Framework 4.5.2";
#elif NET46
=> ".Net Framework 4.6";
#elif NET461
=> ".Net Framework 4.6.1";
#elif NETCORE50
=> ".Net Core 5.0";
#elif DNX11
=> "DNX (.Net Framework 1.1)";
#elif DNX20
=> "DNX (.Net Framework 2.0)";
#elif DNX35
=> "DNX (.Net Framework 3.5)";
#elif DNX40
=> "DNX (.Net Framework 4.0)";
#elif DNX45
=> "DNX (.Net Framework 4.5)";
#elif DNX451
=> "DNX (.Net Framework 4.5.1)";
#elif DNX452
=> "DNX (.Net Framework 4.5.2)";
#elif DNX46
=> "DNX (.Net Framework 4.6)";
#elif DNX461
=> "DNX (.Net Framework 4.6.1)";
#elif DNXCORE50
=> "DNX (.Net Core 5.0)";
#elif DOTNET50 || NETPLATFORM10
=> ".Net Platform Standard 1.0";
#elif DOTNET51 || NETPLATFORM11
=> ".Net Platform Standard 1.1";
#elif DOTNET52 || NETPLATFORM12
=> ".Net Platform Standard 1.2";
#elif DOTNET53 || NETPLATFORM13
=> ".Net Platform Standard 1.3";
#elif DOTNET54 || NETPLATFORM14
=> ".Net Platform Standard 1.4";
#else
        => "Unknown";
#endif
    }
}