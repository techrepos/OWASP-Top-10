using System.Diagnostics;

namespace ABCBankingWebAppAdmin.Services
{
    public class BackupService
    {
        public async Task BackupDB(string backupname)
        {

            string source = Environment.CurrentDirectory + "\\OnlineBank.db";
            string destination = Environment.CurrentDirectory + "\\backups\\" + backupname;
            /*p.StartInfo.Arguments = " /c copy " + source + " " + destination;
            p.StartInfo.FileName = "cmd";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            await p.WaitForExitAsync();*/
            await FileCopyAsync(source, destination);

        }

        public async Task FileCopyAsync(string sourceFileName, string destinationFileName, int bufferSize = 0x1000,
            CancellationToken cancellationToken = default)
        {
            using (var sourceFile = File.OpenRead(sourceFileName))
            {
                using (var destinationFile = File.OpenWrite(destinationFileName))
                {
                    await sourceFile.CopyToAsync(destinationFile, bufferSize, cancellationToken);
                }
            }
        }

    }

    public static class ProcessExtensions
    {
        public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken)
        {
            while (!process.HasExited)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }
}