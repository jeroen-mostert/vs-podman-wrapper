using System.Diagnostics;
using System.Text.RegularExpressions;

ProcessStartInfo processStartInfo = new() {
    FileName = "podman-compose"
};
for (int i = 0; i < args.Length; i++) {
    // Rewrite `--ansi never` to `--no-ansi`
    if (args[i] == "--ansi") {
        ++i;
        if (args[i] == "never") {
            processStartInfo.ArgumentList.Add("--no-ansi");
        }
        continue;
    }
    // Eat `--profile *`
    if (args[i] == "--profile" && args[i + 1] == "*") {
        ++i;
        continue;
    }
    // Eat `--rmi local` (https://github.com/containers/podman-compose/issues/387)
    if (args[i] == "--rmi" && args[i + 1] == "local") {
        ++i;
        continue;
    }
    // Munge paths to Windows mounts (rewrite `C:\foo\bar` to `/foo/bar`)
    if (args[i] == "-f" && args[i + 1].EndsWith(".g.yml")) {
        string yamlPath = args[++i];
        string yaml = File.ReadAllText(yamlPath);
        yaml = Regex.Replace(yaml, @"- [cC]:\\([^:]+)", m => $"- /{m.Groups[1].Value.Replace(@"\", "/")}");
        File.WriteAllText(yamlPath + ".munged", yaml);
        processStartInfo.ArgumentList.Add("-f");
        processStartInfo.ArgumentList.Add(yamlPath + ".munged");
        continue;
    }
    // Rewrite `kill` to `kill --all`
    if (args[i] == "kill" && i == args.Length - 1) {
        processStartInfo.ArgumentList.Add("kill");
        processStartInfo.ArgumentList.Add("--all");
        break;
    }
    processStartInfo.ArgumentList.Add(args[i]);
}

Console.Error.WriteLine($"{processStartInfo.FileName} {string.Join(" ", processStartInfo.ArgumentList)}");
Process.Start(processStartInfo)!.WaitForExit();
