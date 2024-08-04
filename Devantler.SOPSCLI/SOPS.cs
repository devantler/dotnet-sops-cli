using System.Runtime.InteropServices;
using CliWrap;

namespace Devantler.SOPSCLI;

/// <summary>
/// A class to run sops CLI commands.
/// </summary>
public static class SOPS
{
  static Command Command => GetCommand();

  internal static Command GetCommand(PlatformID? platformID = default, Architecture? architecture = default, string? runtimeIdentifier = default)
  {
    platformID ??= Environment.OSVersion.Platform;
    architecture ??= RuntimeInformation.ProcessArchitecture;
    runtimeIdentifier ??= RuntimeInformation.RuntimeIdentifier;

    string binary = (platformID, architecture, runtimeIdentifier) switch
    {
      (PlatformID.Unix, Architecture.X64, "osx-x64") => "sops-darwin-amd64",
      (PlatformID.Unix, Architecture.Arm64, "osx-arm64") => "sops-darwin-arm64",
      (PlatformID.Unix, Architecture.X64, "linux-x64") => "sops-linux-amd64",
      (PlatformID.Unix, Architecture.Arm64, "linux-arm64") => "sops-linux-arm64",
      (PlatformID.Win32NT, Architecture.X64, "win-x64") => "sops-windows-amd64.exe",
      _ => throw new PlatformNotSupportedException($"Unsupported platform: {Environment.OSVersion.Platform} {RuntimeInformation.ProcessArchitecture}"),
    };
    return Cli.Wrap($"{AppContext.BaseDirectory}assets{Path.DirectorySeparatorChar}binaries{Path.DirectorySeparatorChar}{binary}");
  }

  /// <summary>
  /// Decrypt a file using SOPS.
  /// </summary>
  /// <param name="filePath">The path to the file to decrypt.</param>
  /// <param name="sopsAgeKeyFilePath">The path to the sops age key file.</param>
  /// <param name="token">The cancellation token.</param>
  public static async Task<int> DecryptAsync(string filePath, string sopsAgeKeyFilePath, CancellationToken token)
  {
    if (!File.Exists(filePath))
    {
      Console.WriteLine($"✕ File '{filePath}' does not exist");
      return 1;
    }
    Environment.SetEnvironmentVariable("SOPS_AGE_KEY_FILE", sopsAgeKeyFilePath);
    var cmd = Command.WithArguments($"-d -i {filePath}");
    var (exitCode, _) = await CLIRunner.CLIRunner.RunAsync(cmd, token, silent: true).ConfigureAwait(false);
    Environment.SetEnvironmentVariable("SOPS_AGE_KEY_FILE", null);
    return exitCode;
  }

  /// <summary>
  /// Encrypt a file using SOPS.
  /// </summary>
  /// <param name="filePath">The path to the file to encrypt.</param>
  /// <param name="sopsAgeKeyFilePath">The path to the sops age key file.</param>
  /// <param name="token">The cancellation token.</param>
  /// <returns>An integer representing the exit code.</returns>
  public static async Task<int> EncryptAsync(string filePath, string sopsAgeKeyFilePath, CancellationToken token)
  {
    if (!File.Exists(filePath))
    {
      Console.WriteLine($"✕ File '{filePath}' does not exist");
      return 1;
    }
    Environment.SetEnvironmentVariable("SOPS_AGE_KEY_FILE", sopsAgeKeyFilePath);
    var cmd = Command.WithArguments($"-e -i {filePath}");
    var (exitCode, _) = await CLIRunner.CLIRunner.RunAsync(cmd, token, silent: true).ConfigureAwait(false);
    Environment.SetEnvironmentVariable("SOPS_AGE_KEY_FILE", null);
    return exitCode;
  }
}