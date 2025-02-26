using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Documents_Approve
{
    public class Action_Documents_Approve : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        EntityReference target = null;
        Entity en = null;
        string base64File = string.Empty;

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            try
            {
                Init().Wait();
            }
            catch (AggregateException ex)
            {
                var inner = ex.InnerExceptions.FirstOrDefault();
                if(inner != null)
                    throw inner;
            }
        }
        private async Task Init()
        {
            try
            {
                this.target = (EntityReference)this.context.InputParameters["Target"];
                this.base64File = !string.IsNullOrWhiteSpace((string)this.context.InputParameters["File"]) ? (string)this.context.InputParameters["File"] : null;
                this.en = this.service.Retrieve(this.target.LogicalName, this.target.Id, new ColumnSet(new string[] { "bsd_name", "bsd_project", "bsd_filewordtemplatedocx", "statuscode" }));
                
                if (((OptionSetValue)this.en["statuscode"]).Value == 100000000) return; //100000000 = approved
                //DownloadFile();
                await Task.WhenAll(
                    updateDocument(),
                    createDocumentTemplate()
                    );
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task updateDocument()
        {
            try
            {
                tracingService.Trace("Start update sts Document");
                Entity enDocument = new Entity(this.target.LogicalName, this.target.Id);
                enDocument["statuscode"] = new OptionSetValue(100000000);
                this.service.Update(enDocument);
                tracingService.Trace("End update sts Document");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task createDocumentTemplate()
        {
            try
            {
                tracingService.Trace("Start create Document template");
                Entity documentTemplate = new Entity("documenttemplate");
                documentTemplate["name"] = this.en.Contains("bsd_name") ? this.en["bsd_name"].ToString() : null;
                documentTemplate["description"] = this.en.Id.ToString();
                documentTemplate["documenttype"] = new OptionSetValue(2); //2 = Word
                documentTemplate["associatedentitytypecode"] = "salesorder";
                if(!string.IsNullOrWhiteSpace(this.base64File)) documentTemplate["content"] = this.base64File;

                // Tạo record trong hệ thống
                this.service.Create(documentTemplate);
                tracingService.Trace("End create Document template");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void DownloadFile()
        {
            InitializeFileBlocksDownloadRequest initializeFileBlocksDownloadRequest = new InitializeFileBlocksDownloadRequest()
            {
                Target = new EntityReference("bsd_documents", new Guid("757e592a-7ff2-ef11-be20-0022485a0a4f")),
                FileAttributeName = "bsd_filewordtemplatedocx"
            };

            var initializeFileBlocksDownloadResponse =
                  (InitializeFileBlocksDownloadResponse)service.Execute(initializeFileBlocksDownloadRequest);

            string fileContinuationToken = initializeFileBlocksDownloadResponse.FileContinuationToken;
            long fileSizeInBytes = initializeFileBlocksDownloadResponse.FileSizeInBytes;
            tracingService.Trace(fileContinuationToken);
            tracingService.Trace(fileSizeInBytes.ToString());

            List<byte> fileBytes = new List<byte>((int)fileSizeInBytes);

            long offset = 0;
            // If chunking is not supported, chunk size will be full size of the file.
            long blockSizeDownload = !initializeFileBlocksDownloadResponse.IsChunkingSupported ? fileSizeInBytes : 4 * 1024 * 1024;

            // File size may be smaller than defined block size
            if (fileSizeInBytes < blockSizeDownload)
            {
                blockSizeDownload = fileSizeInBytes;
            }
            tracingService.Trace(blockSizeDownload.ToString());
            while (fileSizeInBytes > 0)
            {
                // Prepare the request
                DownloadBlockRequest downLoadBlockRequest = new DownloadBlockRequest()
                {
                    BlockLength = blockSizeDownload,
                    FileContinuationToken = fileContinuationToken,
                    Offset = offset
                };

                // Send the request
                var downloadBlockResponse =
                         (DownloadBlockResponse)service.Execute(downLoadBlockRequest);

                // Add the block returned to the list
                fileBytes.AddRange(downloadBlockResponse.Data);

                // Subtract the amount downloaded,
                // which may make fileSizeInBytes < 0 and indicate
                // no further blocks to download
                fileSizeInBytes -= (int)blockSizeDownload;
                // Increment the offset to start at the beginning of the next block.
                offset += blockSizeDownload;
            }
            string base64String = Convert.ToBase64String(fileBytes.ToArray());
            tracingService.Trace(base64String);
        }
    }
}
