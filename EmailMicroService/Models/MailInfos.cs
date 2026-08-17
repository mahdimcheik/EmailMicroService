namespace EmailMicroService.Models
{
    public record MailAttachment(string FileName, byte[] Content, string? ContentType = null);

    public record MailInfos(
        string From,
        string To,
        string Subject,
        string Body,
        IReadOnlyList<MailAttachment>? Attachments = null
    );
}
