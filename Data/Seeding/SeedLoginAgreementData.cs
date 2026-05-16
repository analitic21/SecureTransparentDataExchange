using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Data.Seeding;

public static class SeedLoginAgreementData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (context.LoginAgreements.Any()) return;

        var agreement = new LoginAgreement
        {
            Title = "User Agreement (GDPR Compliance)",
            Version = "1.0",
            Content = "This agreement outlines your rights and our responsibilities under the GDPR.",
            CreatedAt = DateTime.UtcNow,
            IsLatest = true,
            Articles =
            [
                new() { Title = "Article 5", Content = "Personal data shall be processed lawfully, fairly and transparently." },
                new() { Title = "Article 6", Content = "Processing is lawful only when at least one legal basis applies." },
                new() { Title = "Article 7", Content = "Consent must be freely given, specific, informed and unambiguous." },
                new() { Title = "Article 9", Content = "Special categories require the data-subject's explicit consent." },
                new() { Title = "Article 12", Content = "Transparent information about data-subject rights." },
                new() { Title = "Article 13", Content = "Information when data are collected from the subject." },
                new() { Title = "Article 14", Content = "Information when data are obtained from a third party." },
                new() { Title = "Article 17", Content = "Right to erasure (“right to be forgotten”)." },
                new() { Title = "Article 22", Content = "Right not to be subject to a solely automated decision." }
            ]
        };

        context.LoginAgreements.Add(agreement);
        await context.SaveChangesAsync();
    }
}
