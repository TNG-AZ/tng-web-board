using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.IO.Compression;
using TNG.Web.Board.Data;
using TNG.Web.Board.Services;

namespace TNG.Web.Board.Pages.Signatures
{
    [Authorize(Roles = "Boardmember")]
    public partial class ManageSignatures
    {

#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private IJSRuntime js { get; set; }
#nullable disable

        private async Task GetSignaturesZip()
        {
            using (var compressedFileStream = new MemoryStream())
            {
                //Create an archive and store the stream in memory.
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
                {
                    var sigsBySceneName = context.Signatures.GroupBy(s => s.SceneName.Trim().ToLower());
                    foreach (var sigGroup in sigsBySceneName)
                    {
                        var num = 1;
                        var sceneName = sigGroup.Key;
                        foreach (var sig in sigGroup)
                        {
                            //Create a zip entry for each attachment
                            var zipEntry = zipArchive.CreateEntry($"liabilityForm-{sig.EventId}-{sceneName}-{num}.pdf");

                            //Get the stream of the attachment
                            using (var originalFileStream = new MemoryStream(sig.SignedForm))
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                //Copy the attachment stream to the zip entry stream
                                originalFileStream.CopyTo(zipEntryStream);
                            }
                        }

                    }
                }

                using var streamRef = new DotNetStreamReference(stream: new MemoryStream(compressedFileStream.ToArray()));
                await js.InvokeVoidAsync("downloadFileFromStream", $"liabilityForms_audit.zip", streamRef);
            }
        }
    }
}
