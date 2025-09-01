using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RagBasedChatbot.Helpers
{
    public static class FfmpegConvert
    {
        public static string? OverridePath { get; set; } = @"C:\ffmpeg\bin\ffmpeg.exe";

        public static async Task<MemoryStream> ToWav16kMonoAsync(Stream input, CancellationToken ct = default)
        {
            try
            {
                return await ToWav16kMonoPipeAsync(input, "webm", ct);
            }
            catch (Exception)
            {
                input.Position = 0;
                return await ToWav16kMonoViaTempAsync(input, ".webm", ct);
            }
        }

        private static async Task<MemoryStream> ToWav16kMonoPipeAsync(Stream input, string? forceFormat, CancellationToken ct)
        {
            var ff = ResolveFfmpegPath();
            if (string.IsNullOrWhiteSpace(ff) || !File.Exists(ff))
                throw new FileNotFoundException("ffmpeg.exe bulunamadı", ff);

            var args = new StringBuilder();
            args.Append("-hide_banner -loglevel error ");
            if (!string.IsNullOrEmpty(forceFormat))
                args.Append($"-f {forceFormat} ");      
            args.Append("-i pipe:0 -vn -sn -ac 1 -ar 16000 -acodec pcm_s16le -f wav pipe:1");

            var psi = new ProcessStartInfo
            {
                FileName               = ff,
                Arguments              = args.ToString(),
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            using var proc = new Process { StartInfo = psi };
            if (!proc.Start())
                throw new InvalidOperationException("ffmpeg başlatılamadı.");

            // stdin → ffmpeg
            var stdin = Task.Run(async () =>
            {
                await input.CopyToAsync(proc.StandardInput.BaseStream, ct);
                await proc.StandardInput.BaseStream.FlushAsync(ct);
                proc.StandardInput.Close();
            }, ct);

            var wav = new MemoryStream();
            var stdout = proc.StandardOutput.BaseStream.CopyToAsync(wav, ct);

            var stderrSb = new StringBuilder();
            var stderr = Task.Run(async () =>
            {
                string? line;
                while ((line = await proc.StandardError.ReadLineAsync()) is not null)
                    stderrSb.AppendLine(line);
            }, ct);

            await Task.WhenAll(stdin, stdout, stderr);
            await proc.WaitForExitAsync(ct);

            if (proc.ExitCode != 0)
                throw new Exception($"ffmpeg exitcode={proc.ExitCode}. stderr: {stderrSb}");

            wav.Position = 0;
            return wav;
        }

        public static async Task<MemoryStream> ToWav16kMonoViaTempAsync(Stream input, string ext, CancellationToken ct = default)
        {
            var ff = ResolveFfmpegPath();
            if (string.IsNullOrWhiteSpace(ff) || !File.Exists(ff))
                throw new FileNotFoundException("ffmpeg.exe bulunamadı", ff);

            var inPath  = Path.ChangeExtension(Path.GetTempFileName(), ext);
            var outPath = Path.ChangeExtension(Path.GetTempFileName(), ".wav");

            await using (var f = File.Create(inPath))
                await input.CopyToAsync(f, ct);

            var psi = new ProcessStartInfo
            {
                FileName  = ff,
                Arguments = $"-hide_banner -loglevel error -i \"{inPath}\" -vn -sn -ac 1 -ar 16000 -acodec pcm_s16le -f wav \"{outPath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi)!;
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync(ct);

            if (proc.ExitCode != 0)
                throw new Exception($"ffmpeg exitcode={proc.ExitCode}. stderr: {stderr}");

            var bytes = await File.ReadAllBytesAsync(outPath, ct);
            try { File.Delete(inPath); } catch { }
            try { File.Delete(outPath); } catch { }

            return new MemoryStream(bytes, writable: false);
        }

        private static string ResolveFfmpegPath()
        {
            if (!string.IsNullOrWhiteSpace(OverridePath) && File.Exists(OverridePath)) return OverridePath!;
            var env = Environment.GetEnvironmentVariable("FFMPEG_PATH");
            if (!string.IsNullOrWhiteSpace(env) && File.Exists(env)) return env;
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\ffmpeg\bin\ffmpeg.exe" : "/usr/bin/ffmpeg";
        }
    }
}
