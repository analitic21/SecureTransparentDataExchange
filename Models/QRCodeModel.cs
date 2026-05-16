using System.ComponentModel.DataAnnotations;

public class QRCodeModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string QrCodeBase64 { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // optional связь (оставляем)
    public int? TwoFactorAuthModelId { get; set; }
}
